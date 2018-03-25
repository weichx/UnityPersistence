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
                string[] split = snapshot.Serialize().Split(new[] {Environment.NewLine}, StringSplitOptions.None);
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
                string serialized = snapshot.Serialize();
                string[] split = serialized.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                Snapshot<TypeTest> deserialized = Snapshot<TypeTest>.FromString(serialized);
                TypeTest y = deserialized.Deserialize();
                Assert.AreEqual(typeof(string), y.typeVal);
            }


            struct Thing {

                public float x;
                public float y;

            }
            
            class TypeToGoMissing {

                public float x;
                public Thing thing;

            }

            class TypeMissingInObject {

                public TypeToGoMissing missing;
                public Thing here;

            }

            [Test]
            public void HandleTypeMissingInObject() {
                TypeMissingInObject target = new TypeMissingInObject();
                target.missing = new TypeToGoMissing();
                target.missing.x = 100;
                target.here.x = 100;
                target.here.y = 200;
                Snapshot<TypeMissingInObject> snapshot = new Snapshot<TypeMissingInObject>(target);
                string serialized = snapshot.Serialize();
                serialized = serialized.Replace("TypeToGoMissing", "TypeNowMissing");
                TypeMissingInObject y = Snapshot<TypeMissingInObject>.Deserialize(serialized);
                Assert.IsNull(y.missing);
                Assert.AreEqual(y.here.x, 100);
                Assert.AreEqual(y.here.y, 200);
            }

            class TypeMissingInList {

                public List<object> list;

            }
            
            [Test]
            public void HandleTypeMissingInList() {
                TypeMissingInList target = new TypeMissingInList();
                target.list = new List<object>();
                target.list.Add(200f);
                target.list.Add(new TypeToGoMissing());
                target.list.Add(new TypeToGoMissing());
                target.list.Add(100f);
                Snapshot<TypeMissingInList> snapshot = new Snapshot<TypeMissingInList>(target);

                string serialized = snapshot.Serialize();
                serialized = serialized.Replace("TypeToGoMissing", "TypeNowMissing");
                TypeMissingInList y = Snapshot<TypeMissingInList>.Deserialize(serialized);
                Assert.IsNotNull(y.list);
                Assert.AreEqual(2, y.list.Count);
                Assert.AreEqual(200, y.list[0]);
                Assert.AreEqual(100, y.list[1]);
            }

            class TypeMissingInArray {

                public object[] list;

            }
            
            [Test]
            public void HandleTypeMissingInArray() {
                TypeMissingInArray target = new TypeMissingInArray();
                target.list = new object[4];
                target.list[0] = (200f);
                target.list[1] = (new TypeToGoMissing());
                target.list[2] = (new TypeToGoMissing());
                target.list[3] = (100f);
                Snapshot<TypeMissingInArray> snapshot = new Snapshot<TypeMissingInArray>(target);

                string serialized = snapshot.Serialize();
                serialized = serialized.Replace("TypeToGoMissing", "TypeNowMissing");
                TypeMissingInArray y = Snapshot<TypeMissingInArray>.Deserialize(serialized);
                Assert.IsNotNull(y.list);
                Assert.AreEqual(2, y.list.Length);
                Assert.AreEqual(200, y.list[0]);
                Assert.AreEqual(100, y.list[1]);
            }
            
            //todo -- support delegates

            [Test]
            public void HandleDictionary() {
                Dictionary<int, string> target = new Dictionary<int, string>();
                target[0] = "zero";
                target[1] = "one";
                target[2] = "two";
                Snapshot<Dictionary<int, string>> snapshot = new Snapshot<Dictionary<int, string>>(target);
                string serialized = snapshot.Serialize();
                Snapshot<Dictionary<int, string>> deserialized = Snapshot<Dictionary<int, string>>.FromString(serialized);
                Dictionary<int, string> y = deserialized.Deserialize();
                Assert.AreEqual(y[0], "zero");
                Assert.AreEqual(y[1], "one");
                Assert.AreEqual(y[2], "two");
            }

            class StringTest {

                public string strVal;

            }
            
            [Test]
            public void HandlesLongStringsWithBreaks() {
                StringTest target = new StringTest();
                string strVal = @"

                    this is 
    a complicated

    string


                ";
                target.strVal = strVal;
                Snapshot<StringTest> snapshot = new Snapshot<StringTest>(target);
                StringTest x = snapshot.Deserialize();
                Assert.AreEqual(strVal, x.strVal);
                Snapshot<StringTest> deserialized = Snapshot<StringTest>.FromString(snapshot.Serialize());
                StringTest y = deserialized.Deserialize();
                Assert.AreEqual(strVal, y.strVal);

            }

            class PrivateThing {

                private int notMe = 1;
                public int me = 1;

                public int NotMe {
                    get { return notMe; }
                    set { notMe = value; }
                }
            }

            [Test]
            public void SkipsPrivateFields() {
                PrivateThing target = new PrivateThing();
                target.NotMe = 100;
                target.me = 20;
                Snapshot<PrivateThing> snapshot = new Snapshot<PrivateThing>(target);
                PrivateThing x = snapshot.Deserialize();
                Assert.AreEqual(20, x.me);
                Assert.AreEqual(1, x.NotMe);
                Snapshot<PrivateThing> deserialized = Snapshot<PrivateThing>.FromString(snapshot.Serialize());
                PrivateThing y = deserialized.Deserialize();
                Assert.AreEqual(20, y.me);
                Assert.AreEqual(1, y.NotMe);
                
            }
        }

    }

}