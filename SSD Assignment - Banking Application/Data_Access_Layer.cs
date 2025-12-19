using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                
            }
        }

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {
                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "SELECT * FROM Bank_Accounts";
                        SqliteDataReader dr = command.ExecuteReader();

                        while (dr.Read())
                        {

                            int accountType = dr.GetInt16(7);

                            if (accountType == Account_Type.Current_Account)
                            {
                                Current_Account ca = new Current_Account();
                                ca.accountNo = dr.GetString(0);
                                ca.name = Cryptography.Decrypt(dr.GetString(1));
                                ca.address_line_1 = Cryptography.Decrypt(dr.GetString(2));
                                ca.address_line_2 = Cryptography.Decrypt(dr.GetString(3));
                                ca.address_line_3 = Cryptography.Decrypt(dr.GetString(4));
                                ca.town = Cryptography.Decrypt(dr.GetString(5));
                                ca.balance = dr.GetDouble(6);
                                ca.overdraftAmount = dr.GetDouble(8);
                                accounts.Add(ca);
                            }
                            else
                            {
                                Savings_Account sa = new Savings_Account();
                                sa.accountNo = dr.GetString(0);
                                sa.name = Cryptography.Decrypt(dr.GetString(1));
                                sa.address_line_1 = Cryptography.Decrypt(dr.GetString(2));
                                sa.address_line_2 = Cryptography.Decrypt(dr.GetString(3));
                                sa.address_line_3 = Cryptography.Decrypt(dr.GetString(4));
                                sa.town = Cryptography.Decrypt(dr.GetString(5));
                                sa.balance = dr.GetDouble(6);
                                sa.interestRate = dr.GetDouble(9);
                                accounts.Add(sa);
                            }


                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation failed securely.");
                }
                

            }
        }

        public String addBankAccount(Bank_Account ba) 
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            if (!Validator.IsValidName(ba.name))
                throw new ArgumentException("Invalid account holder name.");

            accounts.Add(ba);

            try
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText =
                    @"
INSERT INTO Bank_Accounts
(accountNo, name, address_line_1, address_line_2, address_line_3, town,
    balance, accountType, overdraftAmount, interestRate)
VALUES
(@accNo, @name, @a1, @a2, @a3, @town, @balance, @type, @overdraft, @interest)
";

                    command.Parameters.AddWithValue("@accNo", ba.accountNo);
                    command.Parameters.AddWithValue("@name", Cryptography.Encrypt(ba.name));
                    command.Parameters.AddWithValue("@a1", Cryptography.Encrypt(ba.address_line_1));
                    command.Parameters.AddWithValue("@a2", Cryptography.Encrypt(ba.address_line_2));
                    command.Parameters.AddWithValue("@a3", Cryptography.Encrypt(ba.address_line_3));
                    command.Parameters.AddWithValue("@town", Cryptography.Encrypt(ba.town));
                    command.Parameters.AddWithValue("@balance", ba.balance);

                    if (ba is Current_Account ca)
                    {
                        command.Parameters.AddWithValue("@type", Account_Type.Current_Account);
                        command.Parameters.AddWithValue("@overdraft", ca.overdraftAmount);
                        command.Parameters.AddWithValue("@interest", DBNull.Value);
                    }
                    else
                    {
                        Savings_Account sa = (Savings_Account)ba;
                        command.Parameters.AddWithValue("@type", Account_Type.Savings_Account);
                        command.Parameters.AddWithValue("@overdraft", DBNull.Value);
                        command.Parameters.AddWithValue("@interest", sa.interestRate);
                    }

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Operation failed securely.");
            }
           

            Logger.Log(
                Environment.UserName,
                ba.accountNo,
                ba.name,
                "Account Creation"
            );

            return ba.accountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        {
            if (!Validator.IsValidAccountNo(accNo))
                return null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    return ba;
                }

            }

            return null; 
        }

        public bool closeBankAccount(String accNo) 
        {
            if (!ActiveDirectoryHelper.IsUserInAdminGroup())
            {
                Console.WriteLine("Admin approval required.");
                return false;
            }

            if (!Validator.IsValidAccountNo(accNo))
                return false;


            Bank_Account toRemove = null;
            
            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }




            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accNo";
                        command.Parameters.AddWithValue("@accNo", toRemove.accountNo);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation failed securely.");
                }
                

                Logger.Log(
                    Environment.UserName,
                    accNo,
                    toRemove.name,
                    "Account Closure"
                );

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {
            if (!Validator.IsValidAccountNo(accNo))
                return false;

            if (!Validator.IsValidAmount(amountToLodge))
                return false;

            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {

                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accNo";

                        command.Parameters.AddWithValue("@balance", toLodgeTo.balance);
                        command.Parameters.AddWithValue("@accNo", toLodgeTo.accountNo);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation failed securely.");
                    return false;
                }

                

                string reason = amountToLodge > 10000 ? "High-value transaction" : "N/A";

                Logger.Log(
                    Environment.UserName,
                    accNo,
                    toLodgeTo.name,
                    "Lodgement",
                    reason
                );

                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            if (!Validator.IsValidAccountNo(accNo))
                return false;

            if (!Validator.IsValidAmount(amountToWithdraw))
                return false;

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {
                try
                {
                    using (var connection = getDatabaseConnection())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accNo";

                        command.Parameters.AddWithValue("@balance", toWithdrawFrom.balance);
                        command.Parameters.AddWithValue("@accNo", toWithdrawFrom.accountNo);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Operation failed securely.");
                    return false;
                }

                
                string reason = amountToWithdraw > 10000 ? "High-value transaction" : "N/A";

                Logger.Log(
                    Environment.UserName,
                    accNo,
                    toWithdrawFrom.name,
                    "Lodgement",
                    reason
                );

                return true;
            }

        }

    }
}
