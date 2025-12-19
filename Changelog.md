# Part 1 – Data Encryption (ALE)

- Added `Cryptography.cs` to handle encryption and decryption logic with AES-256.
- Updated `addBankAccount()` in `Data_Access_Layer.cs` to encrypt all PII fields before inserting records into the database.
- Updated `loadBankAccounts()` to decrypt PII fields after reading them from the database.
- Added a `.env` file for encryption keys to be loaded securely at application startup.

# Part 2 – Logging

- Created `Logger.cs` to centralise all transaction logging logic.
- Transactions are logged for the following operations:
	- `addBankAccount()`
	- `closeBankAccount()`
	- `withdraw()`
	- `lodge()`
- Each log entry includes teller identity, account number, account holder name, transaction type, timestamp, and optional reason for high-value transactions.

# Part 3 – Authentication

- Added `ActiveDirectoryHelper.cs` to handle authentication and authorisation logic.
- Implemented an Active Directory group membership check at application startup to ensure only users in the Bank Teller group can access the system.
- Added an additional administrator authorisation check inside `closeBankAccount()` to restrict account deletion to users in the Bank Teller Administrator group.
- Added an authentication check inside `Program.cs` to verify user identity before allowing any access to the app.

# Part 4 – Additional Security

SQL Injection Protection:
- Improved SQL queries with parameterised queries in:
	- `addBankAccount()`
	- `closeBankAccount()`
	- `withdraw()`
	- `lodge()`

Input Validation:
- Introduced `InputValidator.cs` to centralise validation logic, includes:
	- Ensuring account numbers are correctly formatted
	- Ensuring monetary values are greater than zero
	- Ensuring customer names are not empty or excessively long
- Validation is done within:
  - `addBankAccount()`
  - `findBankAccountByAccNo()`
  - `closeBankAccount()`
  - `lodge()`
  - `withdraw()`

Data Exposure Reduction:
- Implemented basic data redaction by masking customer names in `Bank_Account.ToString()` to prevent unnecessary exposure of sensitive data.

General Improvements:
- Added targeted `try/catch` blocks around database and cryptographic operations to ensure safe failure behaviour.
- Improved separation of concerns by isolating encryption, logging, validation, and authentication into separate classes.
- Refactored parts of the Data Access Layery.
