using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OopFactory.X12.Parsing.Model;
using System.Threading.Tasks;
using OopFactory.X12.Parsing;
 

namespace ExcelClaimGenerator
{
    public partial class ViewModel
    {
        public readonly string ClaimTypeProfessional = "005010X222A1";
 
        public void ExportExcel()
        {
            CancellationRequested = false;
            IsExporting = true;

            var config = new FileConfigModel() { SenderID = "660001234", SenderIDQualifier = "30" };

            var controlNumber = 1000;
            var parts = new List<List<ClaimLine>>();
            var allIds = new List<ClaimLine>(ExcelList);

            while (allIds.Count > 0)
            {
                int take = allIds.Count() > FileRecordLimit ? FileRecordLimit : allIds.Count();
                List<ClaimLine> part = allIds.Take(take).ToList();
                parts.Add(part);
                allIds.RemoveRange(0, take);
            }

            var batchTimeStamp = DateTime.UtcNow.ToString("yyyyMMddhhmmtt");
 
 
           
            IsLoading = true;
            foreach (var part in parts)
            {
                var partIndex = parts.IndexOf(part) + 1;

                var ix = new Interchange(DateTime.UtcNow, partIndex, true, '~', '*', ':');
                ix.SetElement(12, "00501");
                ix.SetElement(11, "^");

                
                var fg = ix.AddFunctionGroup("HC", DateTime.UtcNow, partIndex, ClaimTypeProfessional);

                ix.InterchangeSenderIdQualifier = "30";
                ix.InterchangeSenderId = "660001234";
                ix.InterchangeReceiverIdQualifier = "30";
                ix.InterchangeReceiverId = "GHP660001234";

                fg.ApplicationReceiversCode = ix.InterchangeReceiverId.Clean();
                fg.ApplicationSendersCode = ix.InterchangeSenderId.Clean();

                foreach (var claim in part)
                {
                    if (claim.IsError)
                    {
                        if (claim.Message?.Contains("Skipped") != true)
                            claim.Message = $"Skipped >{claim.Message}";
                        
                        continue;
                    }
 
                    try
                    {
                        claim.Message = "Processing...";
                        CreateProfessionalTransaction(claim, fg, config, controlNumber);
                        claim.Message = $"OK";
                    }
                    catch (Exception ex)
                    {
                        claim.Message = $"Error {ex.Message}";
                    }

                    controlNumber++;

                    if (CancellationRequested)
                    {
                        LoadingMessage = "Canceling...";
                        break;
                    }
                }

                if (!CancellationRequested)
                {
                    LoadingMessage = $"{controlNumber} of {ExcelList.Count}";

                    var cursorPosition = $"{partIndex}_of_{parts.Count}";                   
                    var fileName = $"EXCEL_837P_" + batchTimeStamp + $"_{cursorPosition}" + ".edi";
                    var fullPath = Path.Combine(FolderDestination, fileName);

                    var text = ix.SerializeToX12(false);
                    var bytes = System.Text.Encoding.ASCII.GetBytes(text);
                    File.WriteAllBytes(fullPath, bytes);
                }
            }

            LoadingMessage = "Done!";
            Task.Delay(500).ContinueWith((a) => { IsLoading = false; });

            if (!CancellationRequested)
            {
                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", FolderDestination);
            }
            else
            {
                CancellationRequested = false;
            }

            IsExporting = false;
        }
    }
}