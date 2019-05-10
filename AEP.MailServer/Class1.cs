using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AEP.MailServer
{
    public class AEPSendMail
    {
        private static string Platform = System.Configuration.ConfigurationManager.AppSettings["Platform"].ToString();

        private static string GetLocalPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /////////////////////////////////////
        public static bool SendMail(string SendMailAddress, string AdminMail, string subject, string MailContent)
        {

            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }
                mailMsg.Subject = string.Format("({0}){1}", Platform, subject);
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;
                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress,"", "", subject, MailContent, ret, errMessage);
            }
            return ret;
        }
        public static bool SendMail_CC(string SendMailAddress, string SendCcMailAddress, string AdminMail, string subject, string MailContent)
        {

            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                string[] toCcAddressList = SendCcMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }
                foreach (string toCcAddress in toCcAddressList)
                {
                    if (toCcAddress.Length > 0)
                    {
                        mailMsg.CC.Add(toCcAddress); //副本抄送
                    }
                }
                mailMsg.Subject = string.Format("({0}){1}", Platform, subject);
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;
                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress, SendCcMailAddress, "", subject, MailContent, ret, errMessage);
            }
            return ret;
        }
        public static bool SendMail(string SendMailAddress, string SendBccMailAddress, string AdminMail, string subject, string MailContent)
        {

            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                string[] toBccAddressList = SendBccMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                        //mailMsg.Bcc.Add(toAddress); //全部都用密件副本抄送
                    }
                }
                foreach (string toBccAddress in toBccAddressList)
                {
                    if (toBccAddress.Length > 0)
                    {
                        mailMsg.Bcc.Add(toBccAddress); //全部都用密件副本抄送
                    }
                }
                mailMsg.Subject = string.Format("({0}){1}", Platform, subject);
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;
                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress, "", SendBccMailAddress, subject, MailContent, ret, errMessage);
            }
            return ret;
        }
        public static bool SendMail(string SendMailAddress, string AdminMail, string subject, string MailContent, string[] attachment)
        {
            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }
                mailMsg.Subject = string.Format("({0}){1}", Platform, subject);
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;

                foreach (string file in attachment)
                {
                    mailMsg.Attachments.Add(new System.Net.Mail.Attachment(file));
                }

                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress,"", "", subject, MailContent, ret, errMessage);
            }
            return ret;
        }
        public static bool SendMail(string SendMailAddress, string AdminMail, string subject, string MailContent, List<MailAttachment> lstMailAttachment)
        {
            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }
                mailMsg.Subject = string.Format("({0}){1}", Platform, subject);
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;

                foreach (var file in lstMailAttachment)
                {
                    if (file.Attachment != null && !string.IsNullOrEmpty(file.FileName))
                    {
                        MemoryStream ms = new MemoryStream(file.Attachment);
                        //mailMsg.Attachments.Add(new System.Net.Mail.Attachment(file));
                        mailMsg.Attachments.Add(new Attachment(ms, file.FileName));
                    }
                }

                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress, "", "", subject, MailContent, ret, errMessage);
            }
            return ret;
        }
        public static bool SendMail(string SendMailAddress, string AdminMail, string subject, string MailContent, List<MailAttachment> lstMailAttachment, string mailImgPath)
        {
            bool ret = false;
            string errMessage = "";
            try
            {
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }
                Attachment ImgAttachment = new Attachment(mailImgPath);
                mailMsg.Subject = subject;
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                MailContent = MailContent.Replace("[00]", ImgAttachment.ContentId);
                mailMsg.Body = MailContent;
                mailMsg.Attachments.Add(ImgAttachment);

                foreach (var file in lstMailAttachment)
                {
                    if (file.Attachment != null && !string.IsNullOrEmpty(file.FileName))
                    {
                        MemoryStream ms = new MemoryStream(file.Attachment);
                        //mailMsg.Attachments.Add(new System.Net.Mail.Attachment(file));
                        mailMsg.Attachments.Add(new Attachment(ms, file.FileName));
                    }
                }

                MySmtp.Send(mailMsg);
                ret = true;
            }
            catch (Exception e)
            {
                ret = false;
                errMessage = e.Message;
            }
            finally
            {
                Log(AdminMail, SendMailAddress, "", "", subject, MailContent, ret, errMessage);
            }
            return ret;
        }

        //////////////////////////////////////////
        private static void Log(string SendFrom, string SendTo, string SendCC, string SendBcc, string Subject, string Message, bool ret, string ErrMessage)
        {
            try
            {

                System.Text.StringBuilder sb = new System.Text.StringBuilder();


                sb.Append(" INSERT INTO [dbo].[Mail_Log] ");
                sb.Append("            ([SendFrom] ");
                sb.Append("            ,[SendTo] ");
                sb.Append("            ,[SendCC] ");
                sb.Append("            ,[SendBcc] ");

                sb.Append("            ,[Subject] ");
                sb.Append("            ,[CreateDate] ");
                sb.Append("            ,[Message] ");
                sb.Append("            ,[RetFlag] ");
                sb.Append("            ,[ErrMessage] )");
                sb.Append("     VALUES ");
                sb.Append("          (@SendFrom ");
                sb.Append("           ,@SendTo ");
                sb.Append("           ,@SendCC ");
                sb.Append("           ,@SendBcc ");

                sb.Append("           ,@Subject ");
                sb.Append("           ,@CreateDate ");
                sb.Append("           ,@Message ");
                sb.Append("           ,@RetFlag ");
                sb.Append("           ,@ErrMessage )");


                List<SqlParameter> lstParms = new List<SqlParameter>();
                lstParms.Add(new SqlParameter("@SendFrom", SendFrom));
                lstParms.Add(new SqlParameter("@SendTo", SendTo));
                lstParms.Add(new SqlParameter("@SendCC", SendCC));
                lstParms.Add(new SqlParameter("@SendBcc", SendBcc));
                lstParms.Add(new SqlParameter("@Subject", Subject));
                lstParms.Add(new SqlParameter("@CreateDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                lstParms.Add(new SqlParameter("@Message", Message));
                lstParms.Add(new SqlParameter("@RetFlag", (ret) ? 1 : 0));
                lstParms.Add(new SqlParameter("@ErrMessage", ErrMessage));



                DBHelp.ExecuteNonQuery(sb.ToString(), lstParms.ToArray());
            }
            catch (Exception ex)
            {
                SendMail4ErrorMessage("simonbw_wang@compal.com; AmyJ_Chen@compal.com", "AEPAdmin@compal.com", "AEP Mail Log Error", ex.Message + ex.StackTrace);

            }

        }
        public static bool SendMail4ErrorMessage(string SendMailAddress, string AdminMail, string subject, string MailContent)
        {

            bool ret = false;

            string SendFrom = string.Empty;
            string SendTo = string.Empty;
            string SendCC = string.Empty;
            string Subject = string.Empty;
            string Message = string.Empty;

            try
            {

                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.79");
                //System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("10.110.15.16");
                MailMessage mailMsg = new MailMessage();
                mailMsg.From = new MailAddress(AdminMail);
                string[] toAddressList = SendMailAddress.Split(new char[] { ';' });
                foreach (string toAddress in toAddressList)
                {
                    if (toAddress.Length > 0)
                    {
                        mailMsg.To.Add(toAddress);
                    }
                }


                //mailMsg.To.Add("simonbw_wang@compal.com");
                mailMsg.Subject = subject;
                mailMsg.IsBodyHtml = true;
                mailMsg.Priority = MailPriority.High;
                mailMsg.Body = MailContent;


                SendFrom = mailMsg.From.Address;
                SendTo = SendMailAddress;
                //SendCC = string.Join(";", cCCTo[i]);
                Subject = mailMsg.Subject;

                MySmtp.Send(mailMsg);
                ret = true;

            }
            catch (Exception ex)
            {
                //throw (e);
                Message = ex.Message;

                ret = false;
            }

            return ret;


        }

    }
}
