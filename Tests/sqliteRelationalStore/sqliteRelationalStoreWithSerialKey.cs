using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Biggy.Data.Sqlite;
using NUnit.Framework;

namespace Tests.Sqlite {
  [TestFixture()]
  [Category("SQLite Relational Store")]
  public class SqliteRelationalStoreWithSerialKey {
    private SqliteDbCore _db;
    private string _filename = "";

    [SetUp]
    public void init() {
      _db = new SqliteDbCore("BiggyTestSQLiteRelational");
      _filename = _db.DBFilePath;
      DropCreateTestTables();
    }

    [TearDown]
    public void Cleanup() {
      GC.Collect();
      GC.WaitForPendingFinalizers();
      File.Delete(_filename);
    }

    private void DropCreateTestTables() {
      string propertyTableSql = ""
        + "CREATE TABLE Property (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, Name text, Address text)";
      _db.TryDropTable("Property");
      _db.TransactDDL(propertyTableSql);
    }

    [Test()]
    public void Relational_Store_Inserts_record_with_serial_id() {
      var propertyStore = new SqliteRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "Watergate Apartments", Address = "2639 I St NW, Washington, D.C. 20037" };
      propertyStore.Add(newProperty);

      var foundProperty = propertyStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundProperty != null && foundProperty.Id == 1);
    }

    [Test()]
    public void Relational_Store_Inserts_range_of_records_with_serial_id() {
      var propertyStore = new SqliteRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newProperty = new Property { Name = "New Apartment " + i, Address = "Some Street in a Lonely Town" };
        myBatch.Add(newProperty);
      }
      propertyStore.Add(myBatch);
      var companies = propertyStore.TryLoadData();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Updates_record_with_serial_id() {
      var PropertyStore = new SqliteRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "John's Luxury Apartments", Address = "16 Property Parkway, Portland, OR 97204" };
      PropertyStore.Add(newProperty);

      // Now go fetch the record again and update:
      string newName = "John's Low-Rent Apartments";
      var foundProperty = PropertyStore.TryLoadData().FirstOrDefault();

      int idToFind = foundProperty.Id;
      foundProperty.Name = newName;
      PropertyStore.Update(foundProperty);
      foundProperty = PropertyStore.TryLoadData().FirstOrDefault(p => p.Id == idToFind);

      Assert.IsTrue(foundProperty != null && foundProperty.Name == newName);
    }

    [Test()]
    public void Relational_Store_Updates_range_of_records_with_serial_id() {
      var PropertyStore = new SqliteRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "John's Luxury Townhomes" + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load, and update:
      var companies = PropertyStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        companies.ElementAt(i).Name = "John's Low-Rent Brick Homes " + i;
      }
      PropertyStore.Update(companies);

      // Reload, and check updated names:
      companies = PropertyStore.TryLoadData().Where(c => c.Name.Contains("Low-Rent")).ToList();
      Assert.IsTrue(companies.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Deletes_record_with_serial_id() {
      var PropertyStore = new SqliteRelationalStore<Property>(_db);
      var newProperty = new Property { Name = "John's High-End Apartments", Address = "16 Property Parkway, Portland, OR 97204" };
      PropertyStore.Add(newProperty);

      // Load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      var foundProperty = companies.FirstOrDefault();
      PropertyStore.Delete(foundProperty);

      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Relational_Store_Deletes_range_of_records_with_serial_id() {
      var PropertyStore = new SqliteRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "Boardwalk " + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = companies.Where(c => c.Id <= qtyToDelete);

      // Delete:
      PropertyStore.Delete(deleteThese);
      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Relational_Store_Deletes_all_records_with_serial_id() {
      var PropertyStore = new SqliteRelationalStore<Property>(_db);
      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new Property { Name = "Park Place" + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      PropertyStore.Add(myBatch);

      // Re-load from back-end:
      var companies = PropertyStore.TryLoadData();
      int qtyAdded = companies.Count;

      // Delete:
      PropertyStore.DeleteAll();
      int remaining = PropertyStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}