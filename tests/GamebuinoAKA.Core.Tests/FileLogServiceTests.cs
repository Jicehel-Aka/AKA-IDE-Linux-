using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class FileLogServiceTests
    {
        [Fact]
        public void Info_WritesFile_CreatesDirectory_AndContainsMessage()
        {
            var log = new FileLogService(new TempPlatformPaths());
            log.Info("bonjour-xyz");
            Assert.True(File.Exists(log.LogFilePath));
            Assert.Contains("bonjour-xyz", File.ReadAllText(log.LogFilePath));
        }

        [Fact]
        public void Line_HasLevelAndTimestamp()
        {
            var log = new FileLogService(new TempPlatformPaths());
            log.Info("x");
            var line = File.ReadAllLines(log.LogFilePath)[0];
            Assert.Contains("INFO", line);
            Assert.Matches(@"\d{4}", line); // année dans l'horodatage
        }

        [Fact]
        public void Error_WithException_WritesExceptionDetails()
        {
            var log = new FileLogService(new TempPlatformPaths());
            log.Error("échec", new InvalidOperationException("boom-xyz"));
            Assert.Contains("boom-xyz", File.ReadAllText(log.LogFilePath));
        }

        [Fact]
        public void ManyWrites_DoNotCorrupt_AllLinesPresent()
        {
            var log = new FileLogService(new TempPlatformPaths());
            for (int i = 0; i < 100; i++) log.Info("L" + i);
            var lines = File.ReadAllLines(log.LogFilePath);
            Assert.Equal(100, lines.Length);
        }

        [Fact]
        public void ConcurrentWrites_AreThreadSafe()
        {
            var log = new FileLogService(new TempPlatformPaths());
            Parallel.For(0, 200, i => log.Info("C" + i));
            Assert.Equal(200, File.ReadAllLines(log.LogFilePath).Length);
        }

        [Fact]
        public void Rotation_MovesOldFile_WhenThresholdExceeded()
        {
            var log = new FileLogService(new TempPlatformPaths(), maxBytes: 200);
            for (int i = 0; i < 100; i++) log.Info("ligne de remplissage " + i);
            Assert.True(File.Exists(log.LogFilePath + ".old"));
        }

        [Fact]
        public void Clear_RemovesLogAndHistory()
        {
            var log = new FileLogService(new TempPlatformPaths(), maxBytes: 200);
            for (int i = 0; i < 100; i++) log.Info("x" + i);
            Assert.True(log.Clear());
            Assert.False(File.Exists(log.LogFilePath));
        }

        [Fact]
        public void Log_Facade_NoOp_BeforeConfigure_DoesNotThrow()
        { Log.Configure(null!); Log.Info("rien"); } // ne doit pas lever

        [Fact]
        public void Log_Facade_Delegates_WhenConfigured()
        {
            var log = new FileLogService(new TempPlatformPaths());
            Log.Configure(log);
            Log.Info("via-facade-xyz");
            Assert.Contains("via-facade-xyz", File.ReadAllText(log.LogFilePath));
        }
    }
}
