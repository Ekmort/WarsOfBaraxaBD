using System; 
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
        private OracleConnection conn;
        private String connexionChaine;
        public AccesBD()
        {
           String serveur = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 172.17.104.127)"
           + "(PORT = 1522)))(CONNECT_DATA =(SERVICE_NAME = WarsOfBaraxa)))";
           connexionChaine = "data source=" + serveur + ";userid=WarsOfBaraxa;password=WarsOfBaraxa";
        }

        public void Connection()
        {
            conn = new OracleConnection(connexionChaine);
            conn.Open();
        }

        public Carte[] ListerDeckJoueur(String NomJoueur,int NoDeck)
        {
            Carte []CarteJoueur = null;
            string sql = "SELECT NomCarte,TypeCarte,Habilete,Ble,Bois,Gem,C.NoCarte FROM CARTE C " +
            "INNER JOIN DeckCarte CD ON C.NoCarte=CD.NoCarte " +
            "INNER JOIN DECK D ON CD.NoDeck=D.NoDeck " +
            "INNER JOIN DECKJOUEUR DJ ON D.NoDeck=DJ.NoDeck WHERE DJ.IdJoueur=" + NomJoueur + " AND DJ.NoDeck=" + NoDeck;
            OracleCommand commandeOracle = new OracleCommand(sql,conn);
            OracleDataReader dataReader = commandeOracle.ExecuteReader();

            if(dataReader.HasRows)
            {
                for(int i=0;dataReader.Read();++i)
                {
                    CarteJoueur[i] = new Carte(dataReader.GetString(0),dataReader.GetString(1),dataReader.GetString(2),dataReader.GetInt32(3),dataReader.GetInt32(4),dataReader.GetInt32(5));
                    if(dataReader.GetString(1) == "Permanents")
                    {
                        string sqlPerm = "SELECT TypePerm,Attaque,Armure,Vie FROM Permanents WHERE NoCarte=" + dataReader.GetInt32(5);
                        OracleCommand commandeOraclePerm = new OracleCommand(sqlPerm,conn);
                        OracleDataReader dataReaderPerm = commandeOraclePerm.ExecuteReader();
                        CarteJoueur[i].perm = new Permanent(dataReaderPerm.GetString(0),dataReaderPerm.GetInt32(1),dataReaderPerm.GetInt32(2),dataReaderPerm.GetInt32(3));
                    }
                }
            }
            return CarteJoueur;
        }
    }
}
