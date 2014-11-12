﻿using System;
using System.IO;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    internal class NbtWriterTest {
        [Test]
        public void ValueTest() {
            // write one named tag for every value type, and read it back
            using (var ms = new MemoryStream()) {
                var writer = new NbtWriter(ms, "root");
                Assert.AreEqual(writer.BaseStream, ms);
                {
                    writer.WriteByte("byte", 1);
                    writer.WriteShort("short", 2);
                    writer.WriteInt("int", 3);
                    writer.WriteLong("long", 4L);
                    writer.WriteFloat("float", 5f);
                    writer.WriteDouble("double", 6d);
                    writer.WriteByteArray("byteArray", new byte[] { 10, 11, 12 });
                    writer.WriteIntArray("intArray", new[] { 20, 21, 22 });
                    writer.WriteString("string", "123");
                }
                writer.EndCompound();
                writer.Finish();

                ms.Position = 0;
                var file = new NbtFile();
                file.LoadFromStream(ms, NbtCompression.None);

                TestFiles.AssertValueTest(file);
            }
        }


        [Test]
        public void ByteArrayFromStream() {
            var data = new byte[64*1024];
            for (int i = 0; i < data.Length; i++) {
                data[i] = unchecked((byte)i);
            }

            using (var ms = new MemoryStream()) {
                var writer = new NbtWriter(ms, "root");
                {
                    byte[] buffer = new byte[1024];
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray("byteArray1", dataStream, data.Length);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray("byteArray2", dataStream, data.Length, buffer);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray("byteArray3", dataStream, 1);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray("byteArray4", dataStream, 1, buffer);
                    }

                    writer.BeginList("innerLists", NbtTagType.ByteArray, 4);
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray(dataStream, data.Length);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray(dataStream, data.Length, buffer);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray(dataStream, 1);
                    }
                    using (var dataStream = new NonSeekableStream(new MemoryStream(data))) {
                        writer.WriteByteArray(dataStream, 1, buffer);
                    }
                    writer.EndList();
                }
                writer.EndCompound();
                writer.Finish();

                ms.Position = 0;
                var file = new NbtFile();
                file.LoadFromStream(ms, NbtCompression.None);
                CollectionAssert.AreEqual(file.RootTag["byteArray1"].ByteArrayValue, data);
                CollectionAssert.AreEqual(file.RootTag["byteArray2"].ByteArrayValue, data);
                Assert.AreEqual(file.RootTag["byteArray3"].ByteArrayValue.Length, 1);
                Assert.AreEqual(file.RootTag["byteArray3"].ByteArrayValue[0], data[0]);
                Assert.AreEqual(file.RootTag["byteArray4"].ByteArrayValue.Length, 1);
                Assert.AreEqual(file.RootTag["byteArray4"].ByteArrayValue[0], data[0]);

                CollectionAssert.AreEqual(file.RootTag["innerLists"][0].ByteArrayValue, data);
                CollectionAssert.AreEqual(file.RootTag["innerLists"][1].ByteArrayValue, data);
                Assert.AreEqual(file.RootTag["innerLists"][2].ByteArrayValue.Length, 1);
                Assert.AreEqual(file.RootTag["innerLists"][2].ByteArrayValue[0], data[0]);
                Assert.AreEqual(file.RootTag["innerLists"][3].ByteArrayValue.Length, 1);
                Assert.AreEqual(file.RootTag["innerLists"][3].ByteArrayValue[0], data[0]);
            }
        }


        [Test]
        public void CompoundListTest() {
            // test writing various combinations of compound tags and list tags
            const string testString = "Come on and slam, and welcome to the jam.";
            using (var ms = new MemoryStream()) {
                var writer = new NbtWriter(ms, "Test");
                {
                    writer.BeginCompound("EmptyCompy"); {}
                    writer.EndCompound();

                    writer.BeginCompound("OuterNestedCompy");
                    {
                        writer.BeginCompound("InnerNestedCompy");
                        {
                            writer.WriteInt("IntTest", 123);
                            writer.WriteString("StringTest", testString);
                        }
                        writer.EndCompound();
                    }
                    writer.EndCompound();

                    writer.BeginList("ListOfInts", NbtTagType.Int, 3);
                    {
                        writer.WriteInt(1);
                        writer.WriteInt(2);
                        writer.WriteInt(3);
                    }
                    writer.EndList();

                    writer.BeginCompound("CompoundOfListsOfCompounds");
                    {
                        writer.BeginList("ListOfCompounds", NbtTagType.Compound, 1);
                        {
                            writer.BeginCompound();
                            {
                                writer.WriteInt("TestInt", 123);
                            }
                            writer.EndCompound();
                        }
                        writer.EndList();
                    }
                    writer.EndCompound();


                    writer.BeginList("ListOfEmptyLists", NbtTagType.List, 3);
                    {
                        writer.BeginList(NbtTagType.List, 0); {}
                        writer.EndList();
                        writer.BeginList(NbtTagType.List, 0); {}
                        writer.EndList();
                        writer.BeginList(NbtTagType.List, 0); {}
                        writer.EndList();
                    }
                    writer.EndList();
                }
                writer.EndCompound();
                writer.Finish();

                ms.Seek(0, SeekOrigin.Begin);
                var file = new NbtFile();
                file.LoadFromStream(ms, NbtCompression.None);
                Console.WriteLine(file.ToString());
            }
        }


        [Test]
        public void ListTest() {
            // write short (1-element) lists of every possible kind
            using (var ms = new MemoryStream()) {
                var writer = new NbtWriter(ms, "Test");
                writer.BeginList("LotsOfLists", NbtTagType.List, 11);
                {
                    writer.BeginList(NbtTagType.Byte, 1);
                    writer.WriteByte(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.ByteArray, 1);
                    writer.WriteByteArray(new byte[] {
                        1
                    });
                    writer.EndList();

                    writer.BeginList(NbtTagType.Compound, 1);
                    writer.BeginCompound();
                    writer.EndCompound();
                    writer.EndList();

                    writer.BeginList(NbtTagType.Double, 1);
                    writer.WriteDouble(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.Float, 1);
                    writer.WriteFloat(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.Int, 1);
                    writer.WriteInt(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.IntArray, 1);
                    writer.WriteIntArray(new[] {
                        1
                    });
                    writer.EndList();

                    writer.BeginList(NbtTagType.List, 1);
                    writer.BeginList(NbtTagType.List, 0);
                    writer.EndList();
                    writer.EndList();

                    writer.BeginList(NbtTagType.Long, 1);
                    writer.WriteLong(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.Short, 1);
                    writer.WriteShort(1);
                    writer.EndList();

                    writer.BeginList(NbtTagType.String, 1);
                    writer.WriteString("ponies");
                    writer.EndList();
                }
                writer.EndList();
                writer.EndCompound();
                writer.Finish();

                ms.Position = 0;
                var reader = new NbtReader(ms);
                Assert.DoesNotThrow(() => reader.ReadAsTag());
            }
        }


        [Test]
        public void WriteTagTest() {
            using (var ms = new MemoryStream()) {
                var writer = new NbtWriter(ms, "root");
                {
                    foreach (NbtTag tag in TestFiles.MakeValueTest().Tags) {
                        writer.WriteTag(tag);
                    }
                    writer.EndCompound();
                    writer.Finish();
                }
                ms.Position = 0;
                var file = new NbtFile();
                long bytesRead = file.LoadFromBuffer(ms.ToArray(), 0, (int)ms.Length, NbtCompression.None);
                Assert.AreEqual(bytesRead, ms.Length);
                TestFiles.AssertValueTest(file);
            }
        }


        [Test]
        public void ErrorTest() {
            byte[] dummyByteArray = { 1, 2, 3, 4, 5 };
            int[] dummyIntArray = { 1, 2, 3, 4, 5 };
            MemoryStream dummyStream = new MemoryStream(dummyByteArray);

            using (var ms = new MemoryStream()) {
                // null constructor parameters, or a non-writable stream
                Assert.Throws<ArgumentNullException>(() => new NbtWriter(null, "root"));
                Assert.Throws<ArgumentNullException>(() => new NbtWriter(ms, null));
                Assert.Throws<ArgumentException>(() => new NbtWriter(new NonWritableStream(), "root"));

                var writer = new NbtWriter(ms, "root");
                {
                    // use negative list size
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.BeginList("list", NbtTagType.Int, -1));
                    writer.BeginList("listOfLists", NbtTagType.List, 1);
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.BeginList(NbtTagType.Int, -1));
                    writer.BeginList(NbtTagType.Int, 0);
                    writer.EndList();
                    writer.EndList();

                    writer.BeginList("list", NbtTagType.Int, 1);

                    // invalid list type
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.BeginList(NbtTagType.End, 0));

                    // call EndCompound when not in a compound
                    Assert.Throws<NbtFormatException>(writer.EndCompound);

                    // end list before all elements have been written
                    Assert.Throws<NbtFormatException>(writer.EndList);

                    // write the wrong kind of tag inside a list
                    Assert.Throws<NbtFormatException>(() => writer.WriteShort(0));

                    // write a named tag where an unnamed tag is expected
                    Assert.Throws<NbtFormatException>(() => writer.WriteInt("NamedInt", 0));

                    // write too many list elements
                    writer.WriteTag(new NbtInt());
                    Assert.Throws<NbtFormatException>(() => writer.WriteInt(0));
                    writer.EndList();

                    // write a null tag
                    Assert.Throws<ArgumentNullException>(() => writer.WriteTag(null));

                    // write an unnamed tag where a named tag is expected
                    Assert.Throws<NbtFormatException>(() => writer.WriteTag(new NbtInt()));
                    Assert.Throws<NbtFormatException>(() => writer.WriteInt(0));

                    // end a list when not in a list
                    Assert.Throws<NbtFormatException>(writer.EndList);

                    // write null values where unacceptable
                    Assert.Throws<ArgumentNullException>(() => writer.WriteString(null));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteString("NullString", null));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteByteArray(null));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteByteArray(null, 5));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteByteArray(null, 5, null));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteByteArray(null, 0, 5));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteByteArray("NullByteArray", null, 0, 5));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteIntArray(null));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteIntArray(null,0,5));
                    Assert.Throws<ArgumentNullException>(() => writer.WriteIntArray("NullIntArray", null,0,5));

                    // trying to write array with out-of-range offset/count
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray(dummyByteArray, -1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray(dummyByteArray, 0, -1));
                    Assert.Throws<ArgumentException>(() => writer.WriteByteArray(dummyByteArray, 0, 6));
                    Assert.Throws<ArgumentException>(() => writer.WriteByteArray(dummyByteArray, 1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray("OutOfRangeByteArray", dummyByteArray, -1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray("OutOfRangeByteArray", dummyByteArray, 0, -1));
                    Assert.Throws<ArgumentException>(() => writer.WriteByteArray("OutOfRangeByteArray", dummyByteArray, 0, 6));
                    Assert.Throws<ArgumentException>(() => writer.WriteByteArray("OutOfRangeByteArray", dummyByteArray, 1, 5));

                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteIntArray(dummyIntArray, -1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteIntArray(dummyIntArray, 0, -1));
                    Assert.Throws<ArgumentException>(() => writer.WriteIntArray(dummyIntArray, 0, 6));
                    Assert.Throws<ArgumentException>(() => writer.WriteIntArray(dummyIntArray, 1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteIntArray("OutOfRangeIntArray", dummyIntArray, -1, 5));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteIntArray("OutOfRangeIntArray", dummyIntArray, 0, -1));
                    Assert.Throws<ArgumentException>(() => writer.WriteIntArray("OutOfRangeIntArray", dummyIntArray, 0, 6));
                    Assert.Throws<ArgumentException>(() => writer.WriteIntArray("OutOfRangeIntArray", dummyIntArray, 1, 5));
                    
                    // out-of-range values for stream-reading overloads of WriteByteArray
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray(dummyStream, -1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => writer.WriteByteArray(dummyStream, -1, dummyByteArray));
                    Assert.Throws<ArgumentException>(() => writer.WriteByteArray(dummyStream, 5, new byte[0]));

                    // trying to read from non-readable stream
                    Assert.Throws<ArgumentException>(
                        () => writer.WriteByteArray("ByteStream", new NonReadableStream(), 0));

                    // finish too early
                    Assert.Throws<NbtFormatException>(writer.Finish);

                    writer.EndCompound();
                    writer.Finish();

                    // write tag after finishing
                    Assert.Throws<NbtFormatException>(() => writer.WriteTag(new NbtInt()));
                }
            }
        }


        class NonWritableStream : MemoryStream {
            public override bool CanWrite {
                get { return false; }
            }


            public override void WriteByte(byte value) {
                throw new NotSupportedException();
            }


            public override void Write(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }
        }


        class NonReadableStream : MemoryStream {
            public override bool CanRead {
                get { return false; }
            }


            public override int ReadByte() {
                throw new NotSupportedException();
            }


            public override int Read(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }
        }
    }
}
