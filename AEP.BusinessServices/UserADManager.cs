using AEP.DataModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace AEP.BusinessServices
{
    
    public class UserADManager
    {
        private static string ID = "compal_oa";
        private static string Password = "compaloa";
        private static string sValue = "";
        static string ldapAddress_compal = "LDAP://tpedcs02:389/DC=Compal,DC=com";
        static string ldapAddress_gi = "LDAP://KSPDCS01:389/DC=gi,DC=Compal,DC=com";
        static string ldapAddress_ks = "LDAP://KSDDCS01.ks.compal.com:389/DC=ks,DC=Compal,DC=com";
        static string ldapAddress_cvc = "LDAP://cvcdcs01:389/DC=cvc,DC=Compal,DC=com";
        static string ldapAddress_cee = "LDAP://ceedcs01.cee.compal.com:389/DC=cee,DC=Compal,DC=com";

        private static string[] arr_domain = { "compal", "gi" };
        //private static string[] arr_domain = { "compal", "gi", "ks", "cvc", "cee" };
        private static string[] arr_ldapAddress = { 
                                           "LDAP://tpedcs02:389/DC=Compal,DC=com" ,
                                           "LDAP://KSPDCS01:389/DC=gi,DC=Compal,DC=com" ,
                                           "LDAP://KSDDCS01.ks.compal.com:389/DC=ks,DC=Compal,DC=com" ,
                                           "LDAP://cvcdcs01:389/DC=cvc,DC=Compal,DC=com" ,
                                           "LDAP://ceedcs01.cee.compal.com:389/DC=cee,DC=Compal,DC=com" 
                                       };


        public UserADManager()
        {



        }
        public static UserADInfo GetUserADInfo(string samaccountname)
        {
            UserADInfo myUserADInfo = null;
            foreach (string domain in arr_domain)
            {
                myUserADInfo = GetUserADInfo(domain, samaccountname);
                if (myUserADInfo != null)
                {
                    break;
                }
            }
            return myUserADInfo;
        }
        public static UserADInfo GetUserADInfo_EmployeeID(string employeeid)
        {
            UserADInfo myUserADInfo = null;
            foreach (string domain in arr_domain)
            {
                myUserADInfo = GetUserADInfo_EmployeeID(domain, employeeid);
                if (myUserADInfo != null)
                {
                    break;
                }
            }
            return myUserADInfo;
        }

        public static UserADInfo GetUserADInfo(string domain, string samaccountname)
        {

            UserADInfo myUserADInfo = null;

            if (HttpContext.Current.Session["UserADInfo"] == null)
            {
                string ldapAddress = string.Empty;
                switch (domain.ToLower())
                {
                    case "compal":
                        ldapAddress = ldapAddress_compal;
                        break;
                    case "gi":
                        ldapAddress = ldapAddress_gi;
                        break;
                    case "ks":
                        ldapAddress = ldapAddress_ks;
                        break;
                    case "cvc":
                        ldapAddress = ldapAddress_cvc;
                        break;
                    case "cee":
                        ldapAddress = ldapAddress_cee;
                        break;
                }

                DirectoryEntry de = new DirectoryEntry(ldapAddress, ID, Password);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(sAMAccountName=" + samaccountname + "))";
                ds.SearchScope = SearchScope.Subtree;
                SearchResult sResultSet = ds.FindOne();
                //foreach (SearchResult sResultSet in ds.FindAll())
                if (sResultSet != null)
                {
                    myUserADInfo = new UserADInfo();
                    myUserADInfo.department = GetProperty(sResultSet, "department");
                    myUserADInfo.departmentnumber = GetProperty(sResultSet, "departmentnumber");
                    myUserADInfo.personaltitle = GetProperty(sResultSet, "personaltitle");
                    myUserADInfo.samaccountname = GetProperty(sResultSet, "samaccountname");
                    myUserADInfo.employeeid = GetProperty(sResultSet, "employeeid");
                    myUserADInfo.displayname = GetProperty(sResultSet, "displayname");
                    myUserADInfo.telephonenumber = GetProperty(sResultSet, "telephonenumber");
                    myUserADInfo.division = GetProperty(sResultSet, "division");
                    myUserADInfo.manager = GetProperty(sResultSet, "manager");
                    myUserADInfo.physicaldeliveryofficename = GetProperty(sResultSet, "physicaldeliveryofficename");
                    myUserADInfo.mail = GetProperty(sResultSet, "mail");
                    myUserADInfo.ipphone = GetProperty(sResultSet, "ipphone");
                    myUserADInfo.cn = GetProperty(sResultSet, "cn");
                    myUserADInfo.c = GetProperty(sResultSet, "c");
                    myUserADInfo.co = GetProperty(sResultSet, "co");

                }
                HttpContext.Current.Session["UserADInfo"] = myUserADInfo;
            }
            else
            {
                myUserADInfo = HttpContext.Current.Session["UserADInfo"] as UserADInfo;
            }

            return myUserADInfo;

        }


        public static UserADInfo GetUserADInfo_NoSession(string samaccountname)
        {
            //List<UserADInfo> lstUserADInfo = new List<UserADInfo>();
            //List<Task> tasks = new List<Task>();
            //ConcurrentBag<UserADInfo> bag = new ConcurrentBag<UserADInfo>();

            //foreach (string domain in arr_domain)
            //{

            //    //var task = Task.Factory.StartNew(() => SomeTask(item));
            //    //tasks.Add(task);


            //    var task = Task.Factory.StartNew(() =>
            //    {
            //        UserADInfo myUserADInfo = GetUserADInfo_NoSession(domain, samaccountname);
            //        if (null != myUserADInfo)
            //        {
            //            bag.Add(myUserADInfo);
            //            lstUserADInfo.Add(myUserADInfo);
            //        }
            //    });
            //    tasks.Add(task);
            //}

            //Task.WaitAll(tasks.ToArray());

            //UserADInfo myUserADInfo1 = lstUserADInfo.FirstOrDefault();

            //bool isSucess = bag.TryPeek(out myUserADInfo1);

            UserADInfo myUserADInfo = null;
            foreach (string domain in arr_domain)
            {
                myUserADInfo = GetUserADInfo_NoSession(domain, samaccountname);
                if (myUserADInfo != null)
                {
                    break;
                }
            }
            return myUserADInfo;
        }

        public static UserADInfo GetUserADInfo_NoSession(string domain, string samaccountname)
        {

            UserADInfo myUserADInfo = null;
            try
            {
                string ldapAddress = string.Empty;
                switch (domain.ToLower())
                {
                    case "compal":
                        ldapAddress = ldapAddress_compal;
                        break;
                    case "gi":
                        ldapAddress = ldapAddress_gi;
                        break;
                    case "ks":
                        ldapAddress = ldapAddress_ks;
                        break;
                    case "cvc":
                        ldapAddress = ldapAddress_cvc;
                        break;
                    case "cee":
                        ldapAddress = ldapAddress_cee;
                        break;
                }

                DirectoryEntry de = new DirectoryEntry(ldapAddress, ID, Password);
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(sAMAccountName=" + samaccountname + "))";
                ds.SearchScope = SearchScope.Subtree;
                SearchResult sResultSet = ds.FindOne();
                //foreach (SearchResult sResultSet in ds.FindAll())
                if (sResultSet != null)
                {
                    myUserADInfo = new UserADInfo();
                    myUserADInfo.department = GetProperty(sResultSet, "department");
                    myUserADInfo.departmentnumber = GetProperty(sResultSet, "departmentnumber");
                    myUserADInfo.personaltitle = GetProperty(sResultSet, "personaltitle");
                    myUserADInfo.samaccountname = GetProperty(sResultSet, "samaccountname");
                    myUserADInfo.employeeid = GetProperty(sResultSet, "employeeid");
                    myUserADInfo.displayname = GetProperty(sResultSet, "displayname");
                    myUserADInfo.telephonenumber = GetProperty(sResultSet, "telephonenumber");
                    myUserADInfo.division = GetProperty(sResultSet, "division");
                    myUserADInfo.manager = GetProperty(sResultSet, "manager");
                    myUserADInfo.physicaldeliveryofficename = GetProperty(sResultSet, "physicaldeliveryofficename");
                    myUserADInfo.mail = GetProperty(sResultSet, "mail");
                    myUserADInfo.ipphone = GetProperty(sResultSet, "ipphone");
                    myUserADInfo.cn = GetProperty(sResultSet, "cn");
                    myUserADInfo.c = GetProperty(sResultSet, "c");
                    myUserADInfo.co = GetProperty(sResultSet, "co");

                }

            }
            catch (Exception ex)
            {

            }
            return myUserADInfo;

        }

        public static UserADInfo GetUserADInfo_EmployeeID(string domain, string employeeid)
        {
            string ldapAddress = string.Empty;
            switch (domain.ToLower())
            {
                case "compal":
                    ldapAddress = ldapAddress_compal;
                    break;
                case "gi":
                    ldapAddress = ldapAddress_gi;
                    break;
                case "ks":
                    ldapAddress = ldapAddress_ks;
                    break;
                case "cvc":
                    ldapAddress = ldapAddress_cvc;
                    break;
                case "cee":
                    ldapAddress = ldapAddress_cee;
                    break;
            }

            UserADInfo myUserADInfo = null;
            DirectoryEntry de = new DirectoryEntry(ldapAddress, ID, Password);
            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(employeeid=" + employeeid + "))";
            ds.SearchScope = SearchScope.Subtree;
            SearchResult sResultSet = ds.FindOne();
            //foreach (SearchResult sResultSet in ds.FindAll())
            if (sResultSet != null)
            {
                myUserADInfo = new UserADInfo();
                myUserADInfo.department = GetProperty(sResultSet, "department");
                myUserADInfo.departmentnumber = GetProperty(sResultSet, "departmentnumber");
                myUserADInfo.personaltitle = GetProperty(sResultSet, "personaltitle");
                myUserADInfo.samaccountname = GetProperty(sResultSet, "samaccountname");
                myUserADInfo.employeeid = GetProperty(sResultSet, "employeeid");
                myUserADInfo.displayname = GetProperty(sResultSet, "displayname");
                myUserADInfo.telephonenumber = GetProperty(sResultSet, "telephonenumber");
                myUserADInfo.division = GetProperty(sResultSet, "division");
                myUserADInfo.manager = GetProperty(sResultSet, "manager");
                myUserADInfo.physicaldeliveryofficename = GetProperty(sResultSet, "physicaldeliveryofficename");
                myUserADInfo.mail = GetProperty(sResultSet, "mail");
                myUserADInfo.ipphone = GetProperty(sResultSet, "ipphone");
                myUserADInfo.cn = GetProperty(sResultSet, "cn");
                myUserADInfo.c = GetProperty(sResultSet, "c");
                myUserADInfo.co = GetProperty(sResultSet, "co");

            }

            return myUserADInfo;

        }

        public static string GetProperty(SearchResult searchResult, string PropertyName)
        {
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static List<Principal> GetCompalAccounts(List<string> DisplayNames)
        {
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            List<Principal> lstPrincipal = new List<Principal>(); ;

            using (var context = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                //using (var user = new UserPrincipal(context))
                //{

                //    user.DisplayName = DisplayName;
                //    using (var searcher = new PrincipalSearcher(user))
                //    {
                //        ((DirectorySearcher)searcher.GetUnderlyingSearcher()).SearchScope = SearchScope.Subtree;

                //        lstPrincipal = searcher.FindAll().ToList();
                //    }
                //}


                List<UserPrincipal> searchPrinciples = new List<UserPrincipal>();
                foreach (var p in DisplayNames)
                {
                    searchPrinciples.Add(new UserPrincipal(context) { DisplayName = p }); //SamAccountName 
                }
                //searchPrinciples.Add(new UserPrincipal(context) { DisplayName = "tom*" });
                //searchPrinciples.Add(new UserPrincipal(context) { DisplayName = "tom*" });
                //searchPrinciples.Add(new UserPrincipal(context) { DisplayName = "tom*" });

                var searcher = new PrincipalSearcher();
                foreach (var item in searchPrinciples)
                {
                    searcher = new PrincipalSearcher(item);
                    lstPrincipal.AddRange(searcher.FindAll());
                }


            }





            return lstPrincipal;
        }

        public static List<Principal> GetCompalAccounts(string SamAccountName)
        {
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            List<Principal> lstPrincipal = new List<Principal>(); ;
            using (var context = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var user = new UserPrincipal(context))
                {

                    user.SamAccountName = SamAccountName;
                    using (var searcher = new PrincipalSearcher(user))
                    {
                        ((DirectorySearcher)searcher.GetUnderlyingSearcher()).SearchScope = SearchScope.Subtree;

                        lstPrincipal = searcher.FindAll().ToList();
                    }
                }
            }
            return lstPrincipal;
        }
        public static List<Principal> GetCompalAccounts_EmployeeId(string EmployeeId)
        {
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            List<Principal> lstPrincipal = new List<Principal>(); ;
            using (var context = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var user = new UserPrincipal(context))
                {

                    user.EmployeeId = EmployeeId;
                    using (var searcher = new PrincipalSearcher(user))
                    {
                        ((DirectorySearcher)searcher.GetUnderlyingSearcher()).SearchScope = SearchScope.Subtree;

                        lstPrincipal = searcher.FindAll().ToList();
                    }
                }
            }
            return lstPrincipal;
        }
        public static List<Principal> GetGIAccounts(string SamAccountName)
        {
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "GI.COMPAL.COM";
            List<Principal> lstPrincipal = new List<Principal>(); ;
            using (var context = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var user = new UserPrincipal(context))
                {
                    user.DisplayName = SamAccountName;

                    using (var searcher = new PrincipalSearcher(user))
                    {
                        ((DirectorySearcher)searcher.GetUnderlyingSearcher()).SearchScope = SearchScope.Subtree;

                        lstPrincipal = searcher.FindAll().ToList();
                    }
                }
            }
            return lstPrincipal;
        }
        public static List<Principal> GetGIAccounts_EmployeeId(string EmployeeId)
        {
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "GI.COMPAL.COM";
            List<Principal> lstPrincipal = new List<Principal>(); ;
            using (var context = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var user = new UserPrincipal(context))
                {
                    user.EmployeeId = EmployeeId;

                    using (var searcher = new PrincipalSearcher(user))
                    {
                        ((DirectorySearcher)searcher.GetUnderlyingSearcher()).SearchScope = SearchScope.Subtree;

                        lstPrincipal = searcher.FindAll().ToList();
                    }
                }
            }
            return lstPrincipal;
        }
        public static List<UserADInfo> GetAccounts(string SamAccountName)
        {
            List<Principal> lstPrincipal_Compal = new List<Principal>(); ;
            List<Principal> lstPrincipal_KS = new List<Principal>(); ;
            List<Task> tasks = new List<Task>() {
            Task.Factory.StartNew(() => {
                lstPrincipal_Compal = GetCompalAccounts(SamAccountName);
                //Console.WriteLine(getAccountsTask);
            })
            //}),
            //Task.Factory.StartNew(() => {
            //    lstPrincipal_KS = GetGIAccounts(SamAccountName);
            //   // Console.WriteLine(getDepositsTask);

            //})

        };
            Task.WaitAll(tasks.ToArray());
            List<Principal> lstPrincipal = new List<Principal>(); ;
            lstPrincipal.AddRange(lstPrincipal_Compal);
            //lstPrincipal.AddRange(lstPrincipal_KS);


            List<UserADInfo> lstUserADInfo = new List<UserADInfo>(); ;

            foreach (var p in lstPrincipal)
            {
                UserADInfo myUserADInfo = new UserADInfo();
                myUserADInfo.displayname = p.DisplayName;
                myUserADInfo.samaccountname = p.SamAccountName;
                myUserADInfo.employeeid = ((UserPrincipal)p).EmployeeId;
                myUserADInfo.mail = ((UserPrincipal)p).EmailAddress;
                myUserADInfo.telephonenumber = ((UserPrincipal)p).VoiceTelephoneNumber;

                lstUserADInfo.Add(myUserADInfo);
            }

            return lstUserADInfo;
        }

        public static List<UserADInfo> GetAccounts_EmployeeId(string EmployeeId)
        {
            List<Principal> lstPrincipal_Compal = new List<Principal>(); ;
            List<Principal> lstPrincipal_KS = new List<Principal>(); ;
            List<Task> tasks = new List<Task>() {
            Task.Factory.StartNew(() => {
                lstPrincipal_Compal = GetCompalAccounts_EmployeeId(EmployeeId);
                //Console.WriteLine(getAccountsTask);
            }),
            Task.Factory.StartNew(() => {
                lstPrincipal_KS = GetGIAccounts_EmployeeId(EmployeeId);
               // Console.WriteLine(getDepositsTask);

            })

        };
            Task.WaitAll(tasks.ToArray());
            List<Principal> lstPrincipal = new List<Principal>(); ;
            lstPrincipal.AddRange(lstPrincipal_Compal);
            lstPrincipal.AddRange(lstPrincipal_KS);


            List<UserADInfo> lstUserADInfo = new List<UserADInfo>(); ;

            foreach (var p in lstPrincipal)
            {
                UserADInfo myUserADInfo = new UserADInfo();
                myUserADInfo.displayname = p.DisplayName;
                myUserADInfo.samaccountname = p.SamAccountName;
                myUserADInfo.employeeid = ((UserPrincipal)p).EmployeeId;
                myUserADInfo.mail = ((UserPrincipal)p).EmailAddress;
                myUserADInfo.telephonenumber = ((UserPrincipal)p).VoiceTelephoneNumber;

                lstUserADInfo.Add(myUserADInfo);
            }

            return lstUserADInfo;
        }


        public static List<string> GetADGroup(string name)
        {

            //Search oU
            string ou = "OU=IT,DC=COMPAL,DC=COM";
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            /////////////////////////

            List<string> lstGroupName = new List<string>();
            // create your domain context
            using (PrincipalContext ctx1 = new PrincipalContext(ContextType.Domain,
                                                        DOMAIN,
                                                        "DC=COMPAL,DC=COM"))
            {
                // define a "query-by-example" principal - here, we search for a GroupPrincipal 
                // and with the name like some pattern
                GroupPrincipal qbeGroup = new GroupPrincipal(ctx1);
                qbeGroup.Name = "#IT*";
                qbeGroup.Name = name+"*";

                // create your principal searcher passing in the QBE principal    
                PrincipalSearcher srch1 = new PrincipalSearcher(qbeGroup);

                // find all matches
                foreach (var found in srch1.FindAll())
                {
                    lstGroupName.Add(found.Name);
                    Console.WriteLine(found);
                    // do whatever here - "found" is of type "Principal"
                }
            }

            return lstGroupName;
        }


        public static List<string> GetUsersInADGroup(string groupName)
        {

            //Search oU
            string ou = "OU=IT,DC=COMPAL,DC=COM";
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            /////////////////////////

            List<string> lstUserName = new List<string>();
            // create your domain context
            using (PrincipalContext ctx1 = new PrincipalContext(ContextType.Domain,
                                                        DOMAIN,
                                                        "DC=COMPAL,DC=COM"))
            {

                GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(ctx1, groupName);

                if (groupPrincipal != null)
                {
                    foreach (UserPrincipal p in groupPrincipal.Members)
                    {
                        Console.WriteLine(p.DisplayName);
                        lstUserName.Add(p.SamAccountName);
                    }
                }
 

            }

            return lstUserName;
        }
        public static bool GheckUsersIsInADGroup(string SamAccountName, string groupName)
        {

            //Search oU
            string ou = "OU=IT,DC=COMPAL,DC=COM";
            string ID = "compal_oa";
            string Password = "compaloa";
            string DOMAIN = "COMPAL.COM";
            /////////////////////////
            bool isMember = false;
            List<string> lstUserName = new List<string>();
            // create your domain context
            using (PrincipalContext ctx1 = new PrincipalContext(ContextType.Domain,
                                                        DOMAIN,
                                                        "DC=COMPAL,DC=COM"))
            {

                UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(ctx1, IdentityType.SamAccountName, SamAccountName);

                isMember = userPrincipal.IsMemberOf(ctx1, IdentityType.Name, groupName);

            }

            return isMember;
        }
    }
}
