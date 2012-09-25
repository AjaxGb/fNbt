﻿using System;
using System.IO;
using System.Text;

namespace LibNbt.Tags {
    public class NbtInt : NbtTag, INbtTagValue<int> {
        public int Value { get; set; }

        public NbtInt() : this( "" ) {}


        [Obsolete( "This constructor will be removed in favor of using NbtInt(string tagName, int value)" )]
        public NbtInt( int value ) : this( "", value ) {}


        public NbtInt( string name, int value = 0 ) {
            Name = name;
            Value = value;
        }


        internal override void ReadTag( Stream readStream ) {
            ReadTag( readStream, true );
        }


        internal override void ReadTag( Stream readStream, bool readName ) {
            if( readName ) {
                var name = new NbtString();
                name.ReadTag( readStream, false );

                Name = name.Value;
            }


            var buffer = new byte[4];
            int totalRead = 0;
            while( ( totalRead += readStream.Read( buffer, totalRead, 4 ) ) < 4 ) {}
            if( BitConverter.IsLittleEndian ) Array.Reverse( buffer );
            Value = BitConverter.ToInt32( buffer, 0 );
        }


        internal override void WriteTag( Stream writeStream ) {
            WriteTag( writeStream, true );
        }


        internal override void WriteTag( Stream writeStream, bool writeName ) {
            writeStream.WriteByte( (byte)NbtTagType.Int );
            if( writeName ) {
                var name = new NbtString( "", Name );
                name.WriteData( writeStream );
            }

            WriteData( writeStream );
        }


        internal override void WriteData( Stream writeStream ) {
            byte[] data = BitConverter.GetBytes( Value );
            if( BitConverter.IsLittleEndian ) Array.Reverse( data );
            writeStream.Write( data, 0, data.Length );
        }


        internal override NbtTagType TagType {
            get { return NbtTagType.Int; }
        }


        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append( "TAG_Int" );
            if( Name.Length > 0 ) {
                sb.AppendFormat( "(\"{0}\")", Name );
            }
            sb.AppendFormat( ": {0}", Value );
            return sb.ToString();
        }
    }
}