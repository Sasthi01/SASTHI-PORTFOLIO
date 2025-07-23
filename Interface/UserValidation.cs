using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace LGPACKAGING_POS_SYSTEM.Interface
{
    class UserValidation
    {
        public static class Validator
        {

            //Validate name
            //Name must not be empty.
            public static bool ValidateName(string name, out string errorMessage)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    errorMessage = "Name cannot be empty.";
                    return false;
                }
                errorMessage = string.Empty;
                return true;
            }


            //Validate email address.
            //formated like so: username@domain.top-leveldomain or yusuf@gmail.com
            public static bool ValidateEmail(string email, out string errorMessage)
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(email, pattern))
                {
                    errorMessage = "Invalid email format. Example: yusuf@gmail.com";
                    return false;
                }
                errorMessage = string.Empty;
                return true;
            }


            //Validate contact number.
            //10 digit only.
            public static bool ValidateContactNumber(string contactNumber, out string errorMessage)
            {
                if (contactNumber.Length != 10 || !contactNumber.All(char.IsDigit))
                {
                    errorMessage = "Contact number must be 10 digits long and numeric.";
                    return false;
                }
                errorMessage = string.Empty;
                return true;
            }

            //Validate password
            //Minimum: 8 characters, 2 numbers, 1 symbol.
            public static bool ValidatePassword(string password, out string errorMessage)
            {
                if (password.Length < 8)
                {
                    errorMessage = "Password must be at least 8 characters long.";
                    return false;
                }

                int numberCount = password.Count(char.IsDigit);
                int symbolCount = password.Count(c => "!@#$%^&".Contains(c));

                if (numberCount < 2)
                {
                    errorMessage = "Password must contain at least 2 numbers.";
                    return false;
                }

                if (symbolCount < 1)
                {
                    errorMessage = "Password must contain at least 1 symbol (!,@,#,$,%,^,&).";
                    return false;
                }

                errorMessage = string.Empty;
                return true;
            }

            
            public static bool ValidatePostalCode(string postalCode, out string errorMessage)
            {
                // Check length
                if (postalCode.Length != 4)
                {
                    errorMessage = "Postal code must be exactly 4 digits.";
                    return false;
                }

                // Check if all characters are digits
                foreach (char c in postalCode)
                {
                    if (!char.IsDigit(c))
                    {
                        errorMessage = "Postal code must only contain numbers.";
                        return false;
                    }
                }

                // Valid postal code
                errorMessage = string.Empty;
                return true;
            }

            //Validate address.
            //Cannot be empty, min:5 character long, certain symbols allowed.
            public static bool ValidateAddress(string address, out string errorMessage)
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    errorMessage = "Address cannot be empty.";
                    return false;
                }

                // Optional: Check length
                if (address.Length < 5)
                {
                    errorMessage = "Address is too short. Please enter a valid address.";
                    return false;
                }

                // Optional: Basic pattern check (letters, numbers, space, and common punctuation)
                string pattern = @"^[a-zA-Z0-9\s.,\-#/]+$";
                if (!Regex.IsMatch(address, pattern))
                {
                    errorMessage = "Address contains invalid characters. Only letters, numbers, spaces, and , . - # / are allowed.";
                    return false;
                }

                errorMessage = string.Empty;
                return true;
            }

            //How to call methods:

            //    string error;

            //// Validate First Name
            //if (!UserValidator.ValidateName(txtFirstName.Text, out error))
            //{
            //    MessageBox.Show("First Name Error: " + error);
            //    return;
            //}

            //// Validate Last Name
            //if (!UserValidator.ValidateName(txtLastName.Text, out error))
            //{
            //    MessageBox.Show("Last Name Error: " + error);
            //    return;
            //}

            //// Validate Email
            //if (!UserValidator.ValidateEmail(txtEmail.Text, out error))
            //{
            //    MessageBox.Show("Email Error: " + error);
            //    return;
            //}
        }
    }
}
