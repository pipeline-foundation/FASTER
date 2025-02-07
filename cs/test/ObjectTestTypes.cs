﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using FASTER.core;
using NUnit.Framework;

namespace FASTER.test
{
    public class MyKey : IFasterEqualityComparer<MyKey>
    {
        public int key;

        public long GetHashCode64(ref MyKey key) => Utility.GetHashCode(key.key);

        public bool Equals(ref MyKey k1, ref MyKey k2) => k1.key == k2.key;

        public override string ToString() => this.key.ToString();
    }

    public class MyKeySerializer : BinaryObjectSerializer<MyKey>
    {
        public override void Deserialize(out MyKey obj) => obj = new MyKey { key = reader.ReadInt32() };

        public override void Serialize(ref MyKey obj) => writer.Write(obj.key);
    }

    public class MyValue : IFasterEqualityComparer<MyValue>
    {
        public int value;

        public long GetHashCode64(ref MyValue k) => Utility.GetHashCode(k.value);

        public bool Equals(ref MyValue k1, ref MyValue k2) => k1.value == k2.value;

        public override string ToString() => this.value.ToString();
    }

    public class MyValueSerializer : BinaryObjectSerializer<MyValue>
    {
        public override void Deserialize(out MyValue obj) => obj = new MyValue { value = reader.ReadInt32() };

        public override void Serialize(ref MyValue obj) => writer.Write(obj.value);
    }

    public class MyInput
    {
        public int value;

        public override string ToString() => this.value.ToString();
    }

    public class MyOutput
    {
        public MyValue value;

        public override string ToString() => this.value.ToString();
    }

    public class MyFunctions : FunctionsBase<MyKey, MyValue, MyInput, MyOutput, Empty>
    {
        public override void InitialUpdater(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value = new MyValue { value = input.value };
        }

        public override bool InPlaceUpdater(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value.value += input.value;
            return true;
        }

        public override bool NeedCopyUpdate(ref MyKey key, ref MyInput input, ref MyValue oldValue, ref MyOutput output) => true;

        public override void CopyUpdater(ref MyKey key, ref MyInput input, ref MyValue oldValue, ref MyValue newValue, ref MyOutput output)
        {
            newValue = new MyValue { value = oldValue.value + input.value };
        }

        public override void ConcurrentReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == default)
                dst = new MyOutput();

            dst.value = value;
        }

        public override bool ConcurrentWriter(ref MyKey key, ref MyValue src, ref MyValue dst)
        {
            dst.value = src.value;
            return true;
        }

        public override void ReadCompletionCallback(ref MyKey key, ref MyInput input, ref MyOutput output, Empty ctx, Status status)
        {
            Assert.AreEqual(Status.OK, status);
            Assert.AreEqual(output.value.value, key.key);
        }

        public override void RMWCompletionCallback(ref MyKey key, ref MyInput input, ref MyOutput output, Empty ctx, Status status)
        {
            Assert.AreEqual(Status.OK, status);
        }

        public override void SingleReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == default)
                dst = new MyOutput();
            dst.value = value;
        }

        public override void SingleWriter(ref MyKey key, ref MyValue src, ref MyValue dst)
        {
            dst = src;
        }
    }

    public class MyFunctions2 : FunctionsBase<MyValue, MyValue, MyInput, MyOutput, Empty>
    {
        public override void InitialUpdater(ref MyValue key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value = new MyValue { value = input.value };
        }

        public override bool InPlaceUpdater(ref MyValue key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value.value += input.value;
            return true;
        }

        public override bool NeedCopyUpdate(ref MyValue key, ref MyInput input, ref MyValue oldValue, ref MyOutput output) => true;

        public override void CopyUpdater(ref MyValue key, ref MyInput input, ref MyValue oldValue, ref MyValue newValue, ref MyOutput output)
        {
            newValue = new MyValue { value = oldValue.value + input.value };
        }

        public override void ConcurrentReader(ref MyValue key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == default)
                dst = new MyOutput();

            dst.value = value;
        }

        public override bool ConcurrentWriter(ref MyValue key, ref MyValue src, ref MyValue dst)
        {
            dst.value = src.value;
            return true;
        }

        public override void ReadCompletionCallback(ref MyValue key, ref MyInput input, ref MyOutput output, Empty ctx, Status status)
        {
            Assert.AreEqual(Status.OK, status);
            Assert.AreEqual(key.value, output.value.value);
        }

        public override void RMWCompletionCallback(ref MyValue key, ref MyInput input, ref MyOutput output, Empty ctx, Status status)
        {
            Assert.AreEqual(Status.OK, status);
        }

        public override void SingleReader(ref MyValue key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == default)
                dst = new MyOutput();
            dst.value = value;
        }

        public override void SingleWriter(ref MyValue key, ref MyValue src, ref MyValue dst)
        {
            dst = src;
        }
    }

    public class MyFunctionsDelete : FunctionsBase<MyKey, MyValue, MyInput, MyOutput, int>
    {
        public override void InitialUpdater(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value = new MyValue { value = input.value };
        }

        public override bool InPlaceUpdater(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value.value += input.value;
            return true;
        }

        public override bool NeedCopyUpdate(ref MyKey key, ref MyInput input, ref MyValue oldValue, ref MyOutput output) => true;

        public override void CopyUpdater(ref MyKey key, ref MyInput input, ref MyValue oldValue, ref MyValue newValue, ref MyOutput output)
        {
            newValue = new MyValue { value = oldValue.value + input.value };
        }

        public override void ConcurrentReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == null)
                dst = new MyOutput();
            dst.value = value;
        }

        public override bool ConcurrentWriter(ref MyKey key, ref MyValue src, ref MyValue dst)
        {
            dst = src;
            return true;
        }

        public override void ReadCompletionCallback(ref MyKey key, ref MyInput input, ref MyOutput output, int ctx, Status status)
        {
            if (ctx == 0)
            {
                Assert.AreEqual(Status.OK, status);
                Assert.AreEqual(key.key, output.value.value);
            }
            else if (ctx == 1)
            {
                Assert.AreEqual(Status.NOTFOUND, status);
            }
        }

        public override void RMWCompletionCallback(ref MyKey key, ref MyInput input, ref MyOutput output, int ctx, Status status)
        {
            if (ctx == 0)
                Assert.AreEqual(Status.OK, status);
            else if (ctx == 1)
                Assert.AreEqual(Status.NOTFOUND, status);
        }

        public override void SingleReader(ref MyKey key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            if (dst == null)
                dst = new MyOutput();

            dst.value = value;
        }

        public override void SingleWriter(ref MyKey key, ref MyValue src, ref MyValue dst)
        {
            dst = src;
        }
    }

    public class MixedFunctions : FunctionsBase<int, MyValue, MyInput, MyOutput, Empty>
    {
        public override void InitialUpdater(ref int key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value = new MyValue { value = input.value };
        }

        public override bool InPlaceUpdater(ref int key, ref MyInput input, ref MyValue value, ref MyOutput output)
        {
            value.value += input.value;
            return true;
        }

        public override bool NeedCopyUpdate(ref int key, ref MyInput input, ref MyValue oldValue, ref MyOutput output) => true;

        public override void CopyUpdater(ref int key, ref MyInput input, ref MyValue oldValue, ref MyValue newValue, ref MyOutput output)
        {
            newValue = new MyValue { value = oldValue.value + input.value };
        }

        public override void ConcurrentReader(ref int key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            dst.value = value;
        }

        public override bool ConcurrentWriter(ref int key, ref MyValue src, ref MyValue dst)
        {
            dst.value = src.value;
            return true;
        }

        public override void SingleReader(ref int key, ref MyInput input, ref MyValue value, ref MyOutput dst)
        {
            dst.value = value;
        }

        public override void SingleWriter(ref int key, ref MyValue src, ref MyValue dst)
        {
            dst = src;
        }
    }

    public class MyLargeValue
    {
        public byte[] value;

        public MyLargeValue()
        {

        }

        public MyLargeValue(int size)
        {
            value = new byte[size];
            for (int i = 0; i < size; i++)
            {
                value[i] = (byte)(size+i);
            }
        }
    }

    public class MyLargeValueSerializer : BinaryObjectSerializer<MyLargeValue>
    {
        public override void Deserialize(out MyLargeValue obj)
        {
            obj = new MyLargeValue();
            int size = reader.ReadInt32();
            obj.value = reader.ReadBytes(size);
        }

        public override void Serialize(ref MyLargeValue obj)
        {
            writer.Write(obj.value.Length);
            writer.Write(obj.value);
        }
    }

    public class MyLargeOutput
    {
        public MyLargeValue value;
    }

    public class MyLargeFunctions : FunctionsBase<MyKey, MyLargeValue, MyInput, MyLargeOutput, Empty>
    {
        public override void ReadCompletionCallback(ref MyKey key, ref MyInput input, ref MyLargeOutput output, Empty ctx, Status status)
        {
            Assert.AreEqual(Status.OK, status);
            for (int i = 0; i < output.value.value.Length; i++)
            {
                Assert.AreEqual((byte)(output.value.value.Length + i), output.value.value[i]);
            }
        }

        public override void SingleReader(ref MyKey key, ref MyInput input, ref MyLargeValue value, ref MyLargeOutput dst)
        {
            dst.value = value;
        }

        public override void ConcurrentReader(ref MyKey key, ref MyInput input, ref MyLargeValue value, ref MyLargeOutput dst)
        {
            dst.value = value;
        }

        public override bool ConcurrentWriter(ref MyKey key, ref MyLargeValue src, ref MyLargeValue dst)
        {
            dst = src;
            return true;
        }

        public override void SingleWriter(ref MyKey key, ref MyLargeValue src, ref MyLargeValue dst)
        {
            dst = src;
        }
    }
}
