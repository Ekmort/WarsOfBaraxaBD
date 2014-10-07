using System; 
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using Oracle.DataAccess.Client;
using warsofbaraxa;

namespace WarsOfBaraxaBD
{
    public class AccesBD
    {
        public OracleConnection conn;
        public String connexionChaine;
        private DataSet DataWarsOfBaraxa;
        public AccesBD()
        {
           String serveur = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 172.17.104.113)"
           + "(PORT = 1521)))(CONNECT_DATA =(SERVICE_NAME = orcl)))";
           connexionChaine = "data source=" + serveur + ";user id=WarsOfBaraxa;password=WarsOfBaraxa";
        }

        public void Connection()
        {
            conn = new OracleConnection();
            conn.Open();
        }

        public void ListerDeckJoueur(String NomJoueur)
        {
            Carte [] CarteJoueur;
            string sql = "SELECT * FROM DECKJOUEUR WHERE IdJoueur=" + NomJoueur;
            OracleCommand commandeOracle = new OracleCommand(sql,conn);
            OracleDataReader dataReader = commandeOracle.ExecuteReader();

            if(dataReader.HasRows)
            {
                for(int i=0;dataReader.Read();++i)
                {
                    CarteJoueur[i] = new Carte(dataReader.GetString(0),;
                }
            }
            
        }
    }
}
