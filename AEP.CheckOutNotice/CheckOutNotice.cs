using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using AEP.MailServer;
using AEP.BusinessEntities;

namespace AEP.CheckOutNotice
{
    public class CheckOutNoticeFunc
    {
        public static void Main(string[] args)
        {
            getCheckOutOverDue();
        }

        public static void getCheckOutOverDue()
        {
            try
            {
                DateTime pastDue = DateTime.Now.AddDays(7);
                using (AEPEntities Entities = new AEPEntities())
                {
                    var AEPAdminUser = from a in Entities.AEPAdmin
                                       select a;

                    string AdminEmail = "";
                    foreach (var admin in AEPAdminUser)
                    {
                        AdminEmail += (admin.UserAD.Trim() + "@compal.com;");
                    }
                    ///////////////////////////////////////////////////

                    string mailTitle = "AEP檔案管理系統 - File has been check-out over 7 days";                    
                    
                    var query = from a in Entities.DocRecord
                                where a.DocStatus == 2 && a.CheckOutDate < pastDue
                                group a by new { a.CheckOutBy } into g
                                select g.Key.CheckOutBy;

                    foreach (var user in query)
                    {
                        var query2 = from a in Entities.DocRecord
                                     where a.DocStatus == 2 && a.CheckOutDate < pastDue && a.CheckOutBy == user
                                     select a;

                        string Recipient = string.Format("{0}@Compal.com;", user);

                        StringBuilder mailContent = new StringBuilder();

                        mailContent.Append("<h3><b>Dear ").Append(user).Append(",").Append("</b></h3><br/>");
                        mailContent.Append("<b>Please check below files: ").Append("</b><br/><br/>");
                        mailContent.Append("<table border='1 solid #666'>").Append("<tr style='background-color: #1c6ea4; color: #fff'>");
                        mailContent.Append("<th>No</th>");
                        mailContent.Append("<th>Doc Name</th>");
                        mailContent.Append("<th>Doc Path</th>");
                        mailContent.Append("<th>Check Out Date</th>");
                        mailContent.Append("</tr>");

                        int i = 1;
                        foreach (var item in query2)
                        {
                            if (i % 2 == 0)
                            {
                                mailContent.Append("<tr style='background-color: #f1f6f9'>");
                            }
                            else
                            {
                                mailContent.Append("<tr>");
                            }                            

                            mailContent.Append("<td>").Append(i).Append("</td>");
                            mailContent.Append("<td>").Append(item.DocName).Append("</td>");
                            mailContent.Append("<td>").Append(item.PhysicalPath).Append("</td>");
                            mailContent.Append("<td>").Append(item.CheckOutDate).Append("</td>");
                            mailContent.Append("</tr>");
                            i++;
                        }
                        mailContent.Append("</table>");

                        mailContent.Append("<br/>");
                        mailContent.Append("本郵件由系統自動發佈,請勿直接回覆此郵件, 相關問題請洽Jessie Yu / Norman Lee");

                        // ==================== AEPSendMail ======================                       
                        AEP.MailServer.AEPSendMail.SendMail_CC(Recipient, AdminEmail, "AEPAdmin@compal.com", mailTitle, mailContent.ToString());

                    }

                }
            }
            catch(Exception ex)
            {
                throw(ex);
            }
        }
    }
}
