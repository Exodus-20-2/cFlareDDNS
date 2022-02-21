//
// Cloudflare Dynamic DNS Service
// With this tool you can automatically update your cloudflare config for your specified website to point to your dynamic IP 
// No more manual changes, no more down times becuase your ISP changed your IP address or your router resetted, from now on everything will be updated automatically
// Made by Exodus, Published on Github
//
// Usage:
// Windows - cFlareDDNS.exe <cloudflare auth key> <domain name> <list of domains seperated by ':' if there are none type false > <cloudflare account email>
// Linux CLI - dotnet cFlareDDNS.dll <cloudflare auth key> <domain name> <list of domains seperated by ':' if there are none type false > <cloudflare account email>
// 
// Example:
// Windows - cFlareDDNS.exe 0kX-uxxxxxxxUw6y7_xxxxx-T2xxxxxxxxxxxxxxa astro.rest api:mail:auth example@gmail.com
// Linux - dotnet cFlareDDNS.dll 0kX-uxxxxxxxUw6y7_xxxxx-T2xxxxxxxxxxxxxxa astro.rest api:mail:auth example@gmail.com
//

namespace cFlareDDNS
{
     internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4) { Console.WriteLine("Missing required parameters\nExample: cFlareDDNS.dll/.exe 0kX-uxxxxxxxUw6y7_xxxxx-T2xxxxxxxxxxxxxxa astro.rest api:mail:auth example@gmail.com"); return; }

            Console.Clear();

            string tempWord = "";
            foreach (char letter in args[2])
            {
                if (letter == ':')
                {
                    Data.Globals.domainRecords.Add(tempWord);
                    tempWord = "";
                }
                tempWord += letter;
            }
            if (Data.Globals.domainRecords.Count == 0) Data.Globals.domainRecords.Add(tempWord);
            

            Functions.Cloudflare cflare = new Functions.Cloudflare(args[1], args[3], Data.Globals.domainRecords, args[0]);
            if (cflare.isKeyValid())
            {
                for (; ; Thread.Sleep(Data.Globals.updateInterval))
                {
                    string domainID = cflare.GetDomainID();
                    foreach (string record in Data.Globals.domainRecords)
                    {
                        dynamic dnsRecords = cflare.GetDnsRecords($"{record}.{args[1]}", domainID);
                        if (dnsRecords["content"].ToString() == Data.Globals.globalIpAddress)
                        {
                            Console.WriteLine($"Skipping DNS Change For '{record}.{args[1]}'");
                        }
                        else
                        {
                            Dictionary<string, dynamic> recordData = new Dictionary<string, dynamic> {
                                { "recordName", $"{record}.{args[1]}" },
                                { "recordTTL", dnsRecords["ttl"] }
                            };
                            cflare.ChangeDns(dnsRecords["id"].ToString(), dnsRecords["zone_id"].ToString(), recordData);

                        }
                    }
                    dynamic domainDnsRecords = cflare.GetDnsRecords($"{args[1]}", domainID);
                    if (domainDnsRecords["content"].ToString() == Data.Globals.globalIpAddress)
                    {
                        Console.WriteLine($"Skipping DNS Change For '{args[1]}'");
                        continue;
                    }
                    Dictionary<string, dynamic> recordsData = new Dictionary<string, dynamic> {
                        { "recordName", $"{args[1]}" },
                        { "recordTTL", domainDnsRecords["ttl"] }
                    };
                    cflare.ChangeDns(domainDnsRecords["id"].ToString(), domainDnsRecords["zone_id"].ToString(), recordsData);
                }
            }
            else
            {
                Console.WriteLine("Provided account authorization key is invalid");
                return;
            }
            


        }
    }
}