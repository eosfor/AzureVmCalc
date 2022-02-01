using AzureVmCalc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace AzureVmCalcTests
{
    [TestClass]
    public class BasicTests
    {

        private static PowerShell? _powershell;

        [ClassInitialize()]
        public static void InitUnitTest(TestContext tc)
        {
            _powershell = PowerShell.Create();
            _powershell.AddCommand("Import-Module")
                .AddParameter("Assembly", System.Reflection.Assembly.GetAssembly(typeof(AzureVmCalc.StartOrToolsModelCalculation)));
            _powershell.Invoke();
            _powershell.Commands.Clear();
        }

        [ClassCleanup()]
        public static void CleanupUnitTest()
        {
            _powershell?.Dispose();
        }

        [TestMethod]
        public void BasicTest()
        {
            _powershell.AddScript(@"$sourceVMs = Import-Csv -Path ..\..\..\data\vmdata.csv; $sourceVMs  | % { $_.cpu = [int]$_.cpu; $_.ram = [int]$_.ram; $_.datadisk = [int]$_.datadisk}; $sourceVMs");
            Collection<PSObject> sourceVms = _powershell.Invoke();
            _powershell.Commands.Clear();

            _powershell.AddScript(@"$targetSizes = import-csv ..\..\..\data\vmCostACUData.csv; $targetSizes");
            Collection<PSObject> targetSizes = _powershell.Invoke();
            _powershell.Commands.Clear();

            _powershell.AddCommand("Start-OrToolsModelCalculation").AddParameters(new Dictionary<string, object>
            {
                {"SourceVM", sourceVms}, {"TargetVM", targetSizes}
            });

            Collection<PSObject> res = _powershell.Invoke();
            _powershell.Commands.Clear();

            Assert.IsNotNull(res);
            Assert.AreEqual(res.Count, 2);
            Assert.AreEqual(((VMMappingResult)res[0].ImmediateBaseObject).VmMappingResult.Length, 87);
            Assert.AreEqual(((VMMappingResult)res[1].ImmediateBaseObject).VmMappingResult.Length, 87);
        }
    }
}