﻿using System;

namespace fNbt.Serialization {
    public static class NbtConvert {
        //==== OBJECT TO TAG ==================================================

        public static NbtTag MakeTag<T>(string tagName, T obj) {
            return MakeTag(tagName, obj, typeof(T), ConversionOptions.Defaults);
        }


        public static NbtTag MakeTag<T>(string tagName, T obj, ConversionOptions options) {
            return MakeTag(tagName, obj, typeof(T), options);
        }


        public static NbtTag MakeTag(string tagName, object obj, Type type) {
            return MakeTag(tagName, obj, type, ConversionOptions.Defaults);
        }


        public static NbtTag MakeTag(string tagName, object obj, Type type, ConversionOptions options) {
            NbtTag tag = SerializationUtil.ConstructTag(type);
            if (tag != null) {
                tag.Name = tagName;
                return FillTag(tag, obj, type, options);
            } else {
                throw new NotSupportedException("Conversion of " + type + " objects is not supported.");
            }
        }


        public static NbtTag FillTag<T>(NbtTag tag, T obj) {
            return FillTag(tag, obj, typeof(T), ConversionOptions.Defaults);
        }


        public static NbtTag FillTag<T>(NbtTag tag, T obj, ConversionOptions options) {
            return FillTag(tag, obj, typeof(T), options);
        }


        public static NbtTag FillTag(NbtTag tag, object obj, Type type ) {
            return FillTag(tag, obj, type, ConversionOptions.Defaults);
        }


        public static NbtTag FillTag(NbtTag tag, object obj, Type type, ConversionOptions options) {
            return new DynamicConverter(type, options).FillTag(obj, tag);
        }


        //==== TAG TO OBJECT ==================================================

        public static T MakeObject<T>(NbtTag tag) {
            return (T)MakeObject(tag, typeof(T), ConversionOptions.Defaults);
        }


        public static T MakeObject<T>(NbtTag tag, ConversionOptions options) {
            return (T)MakeObject(tag, typeof(T), options);
        }


        public static object MakeObject(NbtTag tag, Type type) {
            return MakeObject(tag, type, ConversionOptions.Defaults);
        }


        public static object MakeObject(NbtTag tag, Type type, ConversionOptions options) {
            object instance = Activator.CreateInstance(type); // todo: optimize
            return FillObject(tag, instance, type, options);
        }


        public static T FillObject<T>(NbtTag tag, T obj) {
            return (T)FillObject(tag, obj, typeof(T), ConversionOptions.Defaults);
        }


        public static T FillObject<T>(NbtTag tag, T obj, ConversionOptions options) {
            return (T)FillObject(tag, obj, typeof(T), options);
        }


        public static object FillObject(NbtTag tag, object obj, Type type) {
            return FillObject(tag, obj, type, ConversionOptions.Defaults);
        }


        public static object FillObject(NbtTag tag, object obj, Type type, ConversionOptions options) {
            return new DynamicConverter(type, options).FillObject(obj, tag);
        }


        //==== MAKING CONVERTERS ==================================================

        public static NbtConverter<T> MakeConverter<T>() {
            return new NbtConverter<T>();
        }


        public static NbtConverter MakeConverter(Type valueType) {
            return new NbtConverter(valueType);
        }
    }
}
