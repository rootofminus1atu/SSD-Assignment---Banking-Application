# Part 1

PII Information:
- name
- address_line_1
- address_line_2
- address_line_3
- town

Changes made:
- create Cryptography.cs
- adjust addBankAccount() to encrypt PII
- adjust loadBankAccounts() to decrypt PII

# Part 2

EventLog not working, temporary Console.WriteLine quickfix

Changes made:
- create Logger.cs
- add logging to
	- addBankAccount() 
	- closeBankAccounts()
	- withdraw()
	- lodge()

# Part 3

Changes made:
- create ActiveDirectoryHeloper.cs
- add AD check at the start of the program
- add AD admin check in closeBankAccount()

# Part 4

Other fixes:
- protect against SQL Injection in:
	- addBankAccount()
	- closeBankAccounts()
	- withdraw()
	- lodge()
- input validation
	- checking if a name is valid and not empty
	- allowing amounts only above 0
	- checking if an account ID is valid
- input validators applied to 
	- addBankAccount()
	- findBankAccount()
	- closeBankAccount()
	- lodge()
	- withdraw()
- data redaction (name hidden in BankAccount.ToString())
- some try/catches for db/crypto ops




