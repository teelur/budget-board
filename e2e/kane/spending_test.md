---
mode: testing
url: http://localhost:4940/
max_steps: 70
---

# Track a purchase

## Register and log in
Click "Register here".
Type "alex@example.com" as the Email Address, and "Budget-Test-2026!" as both the Password and the Confirm Password, then click "Register".
Verify the login form with the "Login" button is shown.
Type "alex@example.com" as the Email Address and "Budget-Test-2026!" as the Password, then click "Login".
Verify the Dashboard is shown.

## Add an account
Click "Accounts" in the sidebar.
Click the "+" button next to "Reorder".
Type "Everyday Checking" as the Account Name and "Morgan Bank" as the Institution, then click "Submit".
Close the "Create Account" dialog.
Verify "Morgan Bank" is shown with the "Everyday Checking" account.

## Add a transaction
Click "Transactions" in the sidebar.
Click the "+" button above the transaction list.
Type "Corner Grocery" as the Merchant Name.
Click "Select a category" and choose "Groceries".
Type "-42.50" as the Amount.
Click the Account field, choose "Everyday Checking", then press Tab to close the account list.
Click "Submit".
Close the "Create Transaction" dialog.
Verify "Corner Grocery" is listed with an amount of $42.50.

## Check the budget
Click "Budgets" in the sidebar.
Click the "Unbudgeted" row in the Expenses section to expand it.
Verify "Groceries" is shown under Unbudgeted in the Expenses section.
