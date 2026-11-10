using System;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Api.Configuration;

namespace Vezza.Tests.Api;

[TestClass]
public class JawsDbConnectionStringTests
{
    [TestMethod]
    public void Create_ConverteUrlJawsDbSemExporFormatoUrlAoProvider()
    {
        var result = JawsDbConnectionString.Create(
            "mysql://demo%40user:p%40ss%3Bword@db.example.com:3307/vezza_demo",
            "Required",
            4);
        var parsed = new DbConnectionStringBuilder { ConnectionString = result };

        Assert.AreEqual("db.example.com", parsed["server"]);
        Assert.AreEqual("3307", parsed["port"].ToString());
        Assert.AreEqual("vezza_demo", parsed["database"]);
        Assert.AreEqual("demo@user", parsed["user id"]);
        Assert.AreEqual("p@ss;word", parsed["password"]);
        Assert.AreEqual("Required", parsed["sslmode"]);
        Assert.AreEqual("4", parsed["maximumpoolsize"].ToString());
    }

    [TestMethod]
    public void Create_RejeitaUrlInvalida()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            JawsDbConnectionString.Create("https://db.example.com/vezza"));
    }

    [TestMethod]
    public void Create_LimitaPoolAUmIntervaloSeguro()
    {
        var result = JawsDbConnectionString.Create("mysql://user:pass@db.example.com/vezza", maximumPoolSize: 0);
        var parsed = new DbConnectionStringBuilder { ConnectionString = result };

        Assert.AreEqual("1", parsed["maximumpoolsize"].ToString());
    }
}
