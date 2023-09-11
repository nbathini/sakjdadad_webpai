using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace PorscheUtilities.Utility
{
    public static class CommonFunctions
    {
        public static long GetLoggedInUserId(string bearerToken)
        {
            long loggedInUserId = 0;
            try
            {
                var stream = bearerToken;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(stream) as JwtSecurityToken;
                loggedInUserId = Convert.ToInt64(token.Claims.First(claim => claim.Type == Constants.LoggedInUserId).Value);
            }
            catch (Exception ex)
            {
                loggedInUserId = 1;
            }

            
            return loggedInUserId;
        }

        public static long GetCentreId(string bearerToken)
        {
            long centreId = 0;
            try
            {
                var stream = bearerToken;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(stream) as JwtSecurityToken;
                centreId = Convert.ToInt64(token.Claims.First(claim => claim.Type == Constants.CentreId).Value);
            }
            catch (Exception ex)
            {
                centreId = 1;
            }


            return centreId;
        }

        public static string GetUserRoleId(string bearerToken)
        {
            var role = string.Empty;

            try
            {
                var stream = bearerToken;
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
                role = Convert.ToString(tokenS.Claims.First(claim => claim.Type == "RoleId").Value);
            }
            catch (Exception ex)
            {
                role = string.Empty;
            }

            return role;
        }

        public static string GetUserRole(string bearerToken)
        {
            var role = string.Empty;

            try
            {
                var stream = bearerToken;
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
                role = Convert.ToString(tokenS.Claims.First(claim => claim.Type == "RoleName").Value);
            }
            catch(Exception ex)
            {
                role = string.Empty;
            }
            
            return role;
        }

        public static string GetRole(string bearerToken)
        {
            var role = string.Empty;

            try
            {
                var stream = bearerToken;
                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(stream) as JwtSecurityToken;
                role = Convert.ToString(tokenS.Claims.First(claim => claim.Type == "role").Value);
            }
            catch (Exception ex)
            {
                role = string.Empty;
            }

            return role;
        }

        
        public static string LogErrorMessage(string ViewModelName, string MethodName, Exception ex)
        {

            return Constants.ViewModelLog + ViewModelName + Constants.MethodLog + MethodName + Constants.ErrorLog + GetExceptionString(ex) + Environment.NewLine;
        }
        private static string GetExceptionString(Object ex)
        {
            PropertyInfo[] properties = ex.GetType()
                            .GetProperties();
            List<string> fields = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(ex, null);
                fields.Add(String.Format(
                                 "{0} = {1}",
                                 property.Name,
                                 value != null ? value.ToString() : String.Empty
                ));
            }
            return String.Join("\n", fields.ToArray());
        }

        public static string GenerateRandomKey()
        {
            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var resultToken = new string(
               Enumerable.Repeat(allChar, 32)
               .Select(token => token[random.Next(token.Length)]).ToArray());
            var result = resultToken.ToString();
            return result;
        }
        public static string CreateLinkHtml(string section, string path, string pcImgURL)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td width=""700"">" + Environment.NewLine);
            sb.Append(@"<table cellspacing=""0"" cellpadding=""0"" border=""0"">" + Environment.NewLine);
            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""30""></td>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""640"">" + Environment.NewLine);
            sb.Append(@"<table cellspacing=""0"" cellpadding=""0"" width=""640"" border=""0"">" + Environment.NewLine);
            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td style=""font-family:Arial, Helvetica, sans-serif; font-size: 16px; font-weight: normal; line-height: 22px; letter-spacing: normal; color: #000000;"">" + Environment.NewLine);
            sb.Append(@"<a href =" + path + " ");
            sb.Append(@"style=""font-family:Arial, Helvetica, sans-serif; font-size: 16px;font-weight: normal;line-height: 22px;letter-spacing: normal;color: #0061bd; text-decoration:underline;"" ><img src='" + pcImgURL + "' alt='button' /></a>" + Environment.NewLine);
            sb.Append(" </td>" + Environment.NewLine);
            sb.Append(" </tr>" + Environment.NewLine);
            sb.Append("</table>" + Environment.NewLine);
            sb.Append(" </td>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""30""></td>" + Environment.NewLine);
            sb.Append("</tr>" + Environment.NewLine);
            sb.Append("</table>" + Environment.NewLine);
            sb.Append(" </td>" + Environment.NewLine);
            sb.Append("</tr>" + Environment.NewLine);

            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td width=""700"">" + Environment.NewLine);
            sb.Append(@"<table cellspacing=""0"" cellpadding=""0"" border=""0"">" + Environment.NewLine);
            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""30""></td>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""640"">" + Environment.NewLine);
            sb.Append(@"<table cellspacing=""0"" cellpadding=""0"" width=""640"" border=""0"">" + Environment.NewLine);
            sb.Append("<tr>" + Environment.NewLine);
            sb.Append(@"<td class=""extra-height2"" height=""25"">&nbsp;</td>" + Environment.NewLine);
            sb.Append(" </tr>" + Environment.NewLine);
            sb.Append("</table>" + Environment.NewLine);
            sb.Append(" </td>" + Environment.NewLine);
            sb.Append(@"<td valign=""middle"" bgcolor=""#fff"" width=""30""></td>" + Environment.NewLine);
            sb.Append("</tr>" + Environment.NewLine);
            sb.Append("</table>" + Environment.NewLine);
            sb.Append(" </td>" + Environment.NewLine);
            sb.Append("</tr>" + Environment.NewLine);
            return sb.ToString();
        }
    }
}
