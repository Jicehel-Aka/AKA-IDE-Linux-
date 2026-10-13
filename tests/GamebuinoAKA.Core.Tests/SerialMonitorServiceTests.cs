using System;
using GamebuinoAKA.Core.Services;
using Xunit;

namespace GamebuinoAKA.Core.Tests
{
    public class SerialMonitorServiceTests
    {
        [Fact]
        public void ListPorts_DoesNotThrow_AndReturnsArray()
        {
            using var svc = new SerialMonitorService();
            var ports = svc.ListPorts();
            Assert.NotNull(ports);            // tableau (souvent vide en CI, sans matériel)
        }

        [Fact]
        public void NewService_IsNotOpen()
        {
            using var svc = new SerialMonitorService();
            Assert.False(svc.IsOpen);
            Assert.Null(svc.CurrentPort);
        }

        [Fact]
        public void Close_WhenNotOpen_IsSafe()
        {
            var svc = new SerialMonitorService();
            svc.Close();   // ne doit pas lever
            svc.Dispose();
            Assert.False(svc.IsOpen);
        }

        [Fact]
        public void Open_WithEmptyPort_Throws()
        {
            using var svc = new SerialMonitorService();
            Assert.Throws<InvalidOperationException>(() => svc.Open("", 115200));
        }

        [Fact]
        public void Send_WhenNotConnected_Throws()
        {
            using var svc = new SerialMonitorService();
            Assert.Throws<InvalidOperationException>(() => svc.Send("x"));
        }
    }
}
