using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Weichx.Persistence {

    public class Snapshot_Sepc {

        [TestFixture]
        class BasicTests {

            class TestObject {

                public float value;

                public TestObject() { }

                public TestObject(float value) {
                    this.value = value;
                }

            }

            [Test]
            public void SimpleDeserialize() {
                TestObject target = new TestObject(12.1f);
                Snapshot<TestObject> snapshot = new Snapshot<TestObject>(target);
                TestObject x = snapshot.Deserialize();
                Assert.AreEqual(x.value, 12.1f);
                Assert.AreNotEqual(x, target);
            }

            class TestObjectMultiField {

                public float floatVal = 17.5141f;
                public int intVal = 17;
                public string stringValue = "MightNeed ToSpecial CaseThis";
                public char charValue = 'M';
                public bool boolValue = true;
                public double doubleValue = 42.17;
                public decimal decimialValue = 100000;
                public byte byteValue = 8;
                public sbyte sByteValue = sbyte.MinValue;
                public long longValue = long.MinValue;
                public ulong uLongValue = ulong.MaxValue;
                public short shortVal = -32;
                public ushort ushortVal = 32;

            }

            [Test]
            public void SimpleDeserializeMultipleField() {
                TestObjectMultiField target = new TestObjectMultiField();
                target.intVal = 1;
                target.boolValue = false;
                target.byteValue = 5;
                target.doubleValue = 4144.0f;
                Snapshot<TestObjectMultiField> snapshot = new Snapshot<TestObjectMultiField>(target);
                TestObjectMultiField x = snapshot.Deserialize();
                Assert.AreEqual(x.intVal, 1);
                Assert.AreEqual(x.boolValue, false);
                Assert.AreEqual(x.byteValue, 5);
                Assert.AreEqual(x.doubleValue, 4144.0f);
                Assert.AreNotEqual(x, target);
            }

            [Test]
            public void Serializes() {
                TestObjectMultiField target = new TestObjectMultiField();
                target.intVal = 1;
                target.boolValue = false;
                target.byteValue = 5;
                target.doubleValue = 4144.0f;
                Snapshot<TestObjectMultiField> snapshot = new Snapshot<TestObjectMultiField>(target);
                Snapshot<TestObjectMultiField> deserialized = Snapshot<TestObjectMultiField>.FromString(snapshot.Serialize());
                TestObjectMultiField x = deserialized.Deserialize();
                Assert.AreEqual(x.intVal, 1);
                Assert.AreEqual(x.boolValue, false);
                Assert.AreEqual(x.byteValue, 5);
                Assert.AreEqual(x.doubleValue, 4144.0f);
                Assert.AreNotEqual(x, target);

            }

            class ReferenceTypeTest {

                public TestObject refZero = new TestObject(1);
                public TestObject refOne = new TestObject(2);

            }

            [Test]
            public void HandleReferenceTypes() {
                ReferenceTypeTest target = new ReferenceTypeTest();
                target.refOne.value = 100f;
                Snapshot<ReferenceTypeTest> snapshot = new Snapshot<ReferenceTypeTest>(target);
                ReferenceTypeTest x = snapshot.Deserialize();
                Assert.AreNotEqual(target, x);
                Assert.AreEqual(x.refOne.value, 100f);
                Snapshot<ReferenceTypeTest> deserialized = Snapshot<ReferenceTypeTest>.FromString(snapshot.Serialize());
                ReferenceTypeTest y = deserialized.Deserialize();
                Assert.AreNotEqual(y, x);
                Assert.AreEqual(y.refOne.value, 100f);
            }

            class ArrayPrimitiveTest {

                public string[] strings;

            }

            [Test]
            public void HandlesArraysOfPrimitives() {
                ArrayPrimitiveTest target = new ArrayPrimitiveTest();
                target.strings = new string[] {
                    "one", "two", "three"
                };
                Snapshot<ArrayPrimitiveTest> snapshot = new Snapshot<ArrayPrimitiveTest>(target);
                ArrayPrimitiveTest x = snapshot.Deserialize();
                Assert.AreNotEqual(x, target);
                Assert.AreEqual(x.strings[0], "one");
                Assert.AreEqual(x.strings[1], "two");
                Assert.AreEqual(x.strings[2], "three");
                Snapshot<ArrayPrimitiveTest> deserialized = Snapshot<ArrayPrimitiveTest>.FromString(snapshot.Serialize());
                ArrayPrimitiveTest y = deserialized.Deserialize();
                Assert.AreNotEqual(x, y);
                Assert.AreEqual(y.strings[0], "one");
                Assert.AreEqual(y.strings[1], "two");
                Assert.AreEqual(y.strings[2], "three");
            }

            struct Data {

                public char x;
                public byte b;

                public Data(char x, byte b) {
                    this.x = x;
                    this.b = b;
                }

            }

            class ArrayStructTest {

                public Data[] data;

            }

            [Test]
            public void HandlesArraysOfStructs() {
                ArrayStructTest target = new ArrayStructTest();
                target.data = new Data[] {
                    new Data('x', 1), new Data('y', 2), new Data('z', 3),
                };
                Snapshot<ArrayStructTest> snapshot = new Snapshot<ArrayStructTest>(target);
                ArrayStructTest x = snapshot.Deserialize();
                Assert.AreNotEqual(x, target);
                Assert.AreEqual(x.data[0].x, 'x');
                Assert.AreEqual(x.data[0].b, (byte) 1);
                Assert.AreEqual(x.data[1].x, 'y');
                Assert.AreEqual(x.data[1].b, (byte) 2);
                Assert.AreEqual(x.data[2].x, 'z');
                Assert.AreEqual(x.data[2].b, (byte) 3);
                Snapshot<ArrayStructTest> deserialized = Snapshot<ArrayStructTest>.FromString(snapshot.Serialize());
                ArrayStructTest y = deserialized.Deserialize();
                Assert.AreEqual(y.data[0].x, 'x');
                Assert.AreEqual(y.data[0].b, (byte) 1);
                Assert.AreEqual(y.data[1].x, 'y');
                Assert.AreEqual(y.data[1].b, (byte) 2);
                Assert.AreEqual(y.data[2].x, 'z');
                Assert.AreEqual(y.data[2].b, (byte) 3);
            }

            class DataClass {

                public float prop0;
                public Data data;

                public DataClass() { }

                public DataClass(float prop0, char x, byte b) {
                    this.prop0 = prop0;
                    this.data = new Data(x, b);
                }

            }

            class ArrayClassTest {

                public DataClass[] listOfData;

            }

            [Test]
            public void HandlesArrayOfClasses() {
                ArrayClassTest target = new ArrayClassTest();
                target.listOfData = new DataClass[] {
                    new DataClass(11.5f, 'x', 1), new DataClass(-14.7f, 'y', 2), new DataClass(89.4f, 'z', 3),
                };
                Snapshot<ArrayClassTest> snapshot = new Snapshot<ArrayClassTest>(target);
                ArrayClassTest x = snapshot.Deserialize();
                Assert.AreNotEqual(x, target);
                Assert.AreEqual(x.listOfData[0].prop0, 11.5f);
                Assert.AreEqual(x.listOfData[0].data.x, 'x');
                Assert.AreEqual(x.listOfData[0].data.b, (byte) 1);
                Assert.AreEqual(x.listOfData[1].prop0, -14.7f);
                Assert.AreEqual(x.listOfData[1].data.x, 'y');
                Assert.AreEqual(x.listOfData[1].data.b, (byte) 2);
                Assert.AreEqual(x.listOfData[2].prop0, 89.4f);
                Assert.AreEqual(x.listOfData[2].data.x, 'z');
                Assert.AreEqual(x.listOfData[2].data.b, (byte) 3);
                Snapshot<ArrayClassTest> deserialized = Snapshot<ArrayClassTest>.FromString(snapshot.Serialize());
                ArrayClassTest y = deserialized.Deserialize();
                Assert.AreEqual(y.listOfData[0].prop0, 11.5f);
                Assert.AreEqual(y.listOfData[0].data.x, 'x');
                Assert.AreEqual(y.listOfData[0].data.b, (byte) 1);
                Assert.AreEqual(y.listOfData[1].prop0, -14.7f);
                Assert.AreEqual(y.listOfData[1].data.x, 'y');
                Assert.AreEqual(y.listOfData[1].data.b, (byte) 2);
                Assert.AreEqual(y.listOfData[2].prop0, 89.4f);
                Assert.AreEqual(y.listOfData[2].data.x, 'z');
                Assert.AreEqual(y.listOfData[2].data.b, (byte) 3);
            }

            class ListPrimitiveTest {

                public List<int> primtiveList;

            }
            
            [Test]
            public void HandleList() {
                ListPrimitiveTest target = new ListPrimitiveTest();
                target.primtiveList = new List<int>();
                target.primtiveList.Add(0);
                target.primtiveList.Add(1);
                target.primtiveList.Add(2);
                Snapshot<ListPrimitiveTest> snapshot = new Snapshot<ListPrimitiveTest>(target);
                ListPrimitiveTest x = snapshot.Deserialize();
                Assert.AreEqual(0, x.primtiveList[0]);
                Assert.AreEqual(1, x.primtiveList[1]);
                Assert.AreEqual(2, x.primtiveList[2]);
                Snapshot<ListPrimitiveTest> deserialized = Snapshot<ListPrimitiveTest>.FromString(snapshot.Serialize());
                ListPrimitiveTest y = deserialized.Deserialize();
                Assert.AreEqual(0, y.primtiveList[0]);
                Assert.AreEqual(1, y.primtiveList[1]);
                Assert.AreEqual(2, y.primtiveList[2]);
            }
            
            class TypeTest {

                public Type typeVal = typeof(string);

            }

            [Test]
            public void HandleType() {
                TypeTest target = new TypeTest();
                Snapshot<TypeTest> snapshot = new Snapshot<TypeTest>(target);
                TypeTest x = snapshot.Deserialize();
                Assert.AreEqual(typeof(string), x.typeVal);
            }
        }

    }

}