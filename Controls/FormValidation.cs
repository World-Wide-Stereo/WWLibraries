using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ww.Utilities.Extensions;

namespace Controls
{
	public class FormValidation
	{
	    public static void AddOrRemoveHighlight(Control control, bool shouldHighlight)
	    {
	        control.BackColor = shouldHighlight ? Color.LightCoral : Color.Empty;
	    }

	    public static void textboxNumLimitCheck(TextBox textboxToCheck, double largestNum, ref bool isInputValid)
		{
			if (textboxToCheck.Text.ToDouble() > largestNum)
			{
				textboxToCheck.BackColor = Color.LightCoral;
				isInputValid = false;
			}
			else
			{
				textboxToCheck.BackColor = Color.Empty;
			}
		}

		public static string limitPhoneDigits(ref long longPhoneNum, string strPhoneNum, int digitLimit)
		{
            //As the textbox is modified do not allow more digits than digitLimit
		    longPhoneNum = strPhoneNum.GetDigitsInString();
            
			if (longPhoneNum.ToString().Length > digitLimit)
			{
				int numNonDigits = strPhoneNum.Count(x => !Char.IsDigit(x));
				strPhoneNum = strPhoneNum.Substring(0, digitLimit + numNonDigits);
				strPhoneNum = limitPhoneDigits(ref longPhoneNum, strPhoneNum, digitLimit);
			}

			return strPhoneNum;
		}
	}
}