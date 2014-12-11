﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Oracle.DataAccess.Client;
using warsofbaraxa;

namespace WarsOfBaraxaBD
{
    public class AccesBD
    {
        static private OracleConnection conn;
        static private String connexionChaine;
        static OracleDataReader dataReader;
        public AccesBD()
        {
            String serveur = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = mercure.clg.qc.ca)"
            + "(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = orcl.clg.qc.ca)))";
            connexionChaine = "data source=" + serveur + ";user id=Menardal;password=oracle1";
        }

        public void Connection()
        {
            try
            {
                conn = new OracleConnection(connexionChaine);
            }
            catch (OracleException) { }
        }
        public void Open()
        { 
            if(conn.State.ToString() != "Open")
            {
                try
                {
                    conn.Open();
                }
                catch(OracleException){}
            }
        }
        public void close()
        {
            if (conn.State.ToString() == "Open")
            {
                try
                {
                    conn.Close();
                }
                catch (OracleException) { }
            }            
        }

        public int getNoDeck(string Nomdeck)
        {
            int noDeck = -1;
            string sql = "select * from deck where NomDeck = '"+Nomdeck+"'";
            OracleCommand command = new OracleCommand(sql,conn);
            try
            {
                dataReader = command.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    noDeck = dataReader.GetInt32(0);
                }
                dataReader.Dispose();
            }
            catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }
            return noDeck;
        }
        public Carte[] ListerDeckJoueur(String NomJoueur, int NoDeck)
        {

            Carte[] CarteJoueur = new Carte[40];
            string sql = "SELECT NomCarte,TypeCarte,NVL(Habilete,'null'),Ble,Bois,Gem,C.NoCarte,NombreDeFois FROM CARTE C " +
            "INNER JOIN DeckCarte CD ON C.NoCarte=CD.NoCarte " +
            "INNER JOIN DECK D ON CD.NoDeck=D.NoDeck " +
            "INNER JOIN DECKJOUEUR DJ ON D.NoDeck=DJ.NoDeck WHERE DJ.IdJoueur='" + NomJoueur + "' AND DJ.NoDeck=" + NoDeck;
            OracleCommand commandeOracle = new OracleCommand(sql, conn);

            try
            {
                dataReader = commandeOracle.ExecuteReader();

                if (dataReader.HasRows)
                {
                    int Compteur = 0;
                    while (dataReader.Read())
                    {
                        for (int j = 0; j < dataReader.GetInt32(7); ++j)
                        {
                            CarteJoueur[Compteur] = new Carte(dataReader.GetInt32(6), dataReader.GetString(0), dataReader.GetString(1), dataReader.GetString(2), dataReader.GetInt32(3), dataReader.GetInt32(4), dataReader.GetInt32(5));
                            ++Compteur;
                        }
                    }

                    dataReader.Dispose();
                    dataReader.Close();
                    ListerPermanents(CarteJoueur);
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write(e);
            }
            return CarteJoueur;
        }

        public void ListerPermanents(Carte[] CarteJoueur)
        {
            try
            {
                for (int i = 0; i < CarteJoueur.Length && CarteJoueur[i] != null; ++i)
                {
                    if (CarteJoueur[i].TypeCarte == "Permanents")
                    {
                        string sqlPerm = "SELECT TypePerm,Attaque,Armure,Vie FROM Permanents WHERE NoCarte=" + CarteJoueur[i].NoCarte;
                        OracleCommand commandeOraclePerm = new OracleCommand(sqlPerm, conn);
                        dataReader = commandeOraclePerm.ExecuteReader();
                        dataReader.Read();
                        CarteJoueur[i].perm = new Permanent(dataReader.GetString(0), dataReader.GetInt32(1), dataReader.GetInt32(3), dataReader.GetInt32(2));
                    }
                }
            }
            catch (OracleException)
            { }
            dataReader.Dispose();
        }
        public bool estPresent(string nomAlias, string mdp)
        {
            try
            {
                string sql = "select * from joueur where IdJoueur='" + nomAlias + "' and Pword='" + mdp + "'";
                OracleCommand orac = new OracleCommand(sql, conn);
                dataReader = orac.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Dispose();
                    return true;
                }
                dataReader.Dispose();
            }
            catch (OracleException)
            {
                dataReader.Dispose();
            }
            return false;
        }
        public bool estDejaPresent(string nomAlias)
        {
            try
            {
                string sql = "select * from joueur where IdJoueur='" + nomAlias + "'";
                OracleCommand orac = new OracleCommand(sql, conn);
                dataReader = orac.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Dispose();
                    return true;
                }
                dataReader.Dispose();
            }
            catch (OracleException)
            {
                dataReader.Dispose();
            }
            return false;
        }
        public void AjouterVictoire(string alias)
        {
            try
            {
                OracleCommand orac = new OracleCommand("JOUEURPACKAGE", conn);
                orac.CommandText = "JOUEURPACKAGE.ajouterVictoire";
                orac.CommandType = CommandType.StoredProcedure;

                OracleParameter oraalias = new OracleParameter("pId", OracleDbType.Varchar2);

                oraalias.Value = alias;
                oraalias.Direction = ParameterDirection.Input;

                orac.Parameters.Add(oraalias);

                orac.ExecuteNonQuery();
            }
            catch (OracleException) { }
        }
        public void AjouterDefaite(string alias)
        {
            try
            {
                OracleCommand orac = new OracleCommand("JOUEURPACKAGE", conn);
                orac.CommandText = "JOUEURPACKAGE.ajotuerDefaite";
                orac.CommandType = CommandType.StoredProcedure;

                OracleParameter oraalias = new OracleParameter("pId", OracleDbType.Varchar2);

                oraalias.Value = alias;
                oraalias.Direction = ParameterDirection.Input;

                orac.Parameters.Add(oraalias);

                orac.ExecuteNonQuery();
            }
            catch (OracleException ora)
            { }
        }
        public string getProfil(string alias)
        {
            try
            {
                string sql = "select Victoire,Defaite,idjoueur,Prenom,nom from joueur where IdJoueur='" + alias + "'";
                OracleCommand orac = new OracleCommand(sql, conn);
                dataReader = orac.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    int victoire = dataReader.GetInt32(0);
                    int defaite = dataReader.GetInt32(1);
                    string id = dataReader.GetString(2);
                    string nom = dataReader.GetString(3) + " " + dataReader.GetString(4);
                    dataReader.Dispose();
                    return victoire.ToString() + "," + defaite.ToString() + "," + id + "," + nom;
                }
                dataReader.Dispose();
            }
            catch (OracleException ora)
            { }
            return null;
        }
        public void ajouter(string alias, string mdp, string nom, string prenom)
        {
            try
            {
                OracleCommand orac = new OracleCommand("JOUEURPACKAGE", conn);
                orac.CommandText = "JOUEURPACKAGE.insertJoueur";
                orac.CommandType = CommandType.StoredProcedure;

                OracleParameter oraalias = new OracleParameter("pAlias", OracleDbType.Varchar2);
                OracleParameter oraMDP = new OracleParameter("pPassword", OracleDbType.Varchar2);
                OracleParameter oraNom = new OracleParameter("pNom", OracleDbType.Varchar2);
                OracleParameter oraprenom = new OracleParameter("pPrenom", OracleDbType.Varchar2);

                oraalias.Value = alias;
                oraalias.Direction = ParameterDirection.Input;

                oraMDP.Value = mdp;
                oraMDP.Direction = ParameterDirection.Input;

                oraNom.Value = nom;
                oraNom.Direction = ParameterDirection.Input;

                oraprenom.Value = prenom;
                oraprenom.Direction = ParameterDirection.Input;

                orac.Parameters.Add(oraalias);
                orac.Parameters.Add(oraMDP);
                orac.Parameters.Add(oraNom);
                orac.Parameters.Add(oraprenom);

                orac.ExecuteNonQuery();
            }
            catch (OracleException ora)
            {
                                
            }            
        }
        public string getDeckJoueur(string alias)
        {
            string message = "";
            try
            {
                string sql = "select deck.nomdeck from deck inner join deckjoueur on deck.nodeck = deckjoueur.nodeck where deckjoueur.idjoueur = '" + alias + "'";
                OracleCommand orac = new OracleCommand(sql, conn);
                dataReader = orac.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    message += dataReader.GetString(0);
                    while (dataReader.Read())
                    {
                        message += "," + dataReader.GetString(0);
                    }
                    dataReader.Dispose();
                }
            }
            catch (OracleException ora) { }
            return message;
        }
        public void setBasicDeck(string alias)
        {
            try
            {
                OracleCommand orac = new OracleCommand("JOUEURPACKAGE", conn);
                orac.CommandText = "JOUEURPACKAGE.ajouterBasicDeck";
                orac.CommandType = CommandType.StoredProcedure;

                OracleParameter oraalias = new OracleParameter("pAlias", OracleDbType.Varchar2);

                oraalias.Value = alias;
                oraalias.Direction = ParameterDirection.Input;

                orac.Parameters.Add(oraalias);
                orac.ExecuteNonQuery();
            }
            catch (OracleException ora)
            {

            }              
        }
    }
}
