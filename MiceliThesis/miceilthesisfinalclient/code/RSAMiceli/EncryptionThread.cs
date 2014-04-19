using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RSACryptoPad
{
	public class EncryptionThread
	{
		private ContainerControl containerControl = null;
		private Delegate finishedProcessDelegate = null;
		private Delegate updateTextDelegate = null;
		
		public void Encrypt( object inputObject )
		{
			object[] inputObjects = ( object[] )inputObject;
			containerControl = ( Form ) inputObjects[ 0 ];
			finishedProcessDelegate = ( Delegate ) inputObjects[ 1 ];
			updateTextDelegate = ( Delegate )inputObjects[ 2 ];
			string encryptedString = EncryptString( ( string )inputObjects[ 3 ], ( int )inputObjects[ 4 ], ( string )inputObjects[ 5 ] );
			containerControl.Invoke( updateTextDelegate, new object[] { encryptedString } );
			containerControl.Invoke( finishedProcessDelegate );			
		}

		public void Decrypt( object inputObject )
		{
			object[] inputObjects = ( object[] )inputObject;
			containerControl = ( Form )inputObjects[ 0 ];
			finishedProcessDelegate = ( Delegate )inputObjects[ 1 ];
			updateTextDelegate = ( Delegate )inputObjects[ 2 ];
			string decryptedString = DecryptString( ( string )inputObjects[ 3 ], ( int )inputObjects[ 4 ], ( string )inputObjects[ 5 ] );
			containerControl.Invoke( updateTextDelegate, new object[] { decryptedString } );
			containerControl.Invoke( finishedProcessDelegate );
		}

		public string EncryptString( string inputString, int dwKeySize, string xmlString )
		{
			// TODO: Add Proper Exception Handlers
			RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider( dwKeySize );
			rsaCryptoServiceProvider.FromXmlString( xmlString );
			int keySize = dwKeySize / 8;
			byte[] bytes = Encoding.UTF32.GetBytes( inputString );
			// The hash function in use by the .NET RSACryptoServiceProvider here is SHA1
			// int maxLength = ( keySize ) - 2 - ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
			int maxLength = keySize - 42;
			int dataLength = bytes.Length;
			int iterations = dataLength / maxLength;
			StringBuilder stringBuilder = new StringBuilder();
			for( int i = 0; i <= iterations; i++ )
			{
				byte[] tempBytes = new byte[ ( dataLength - maxLength * i > maxLength ) ? maxLength : dataLength - maxLength * i ];
				Buffer.BlockCopy( bytes, maxLength * i, tempBytes, 0, tempBytes.Length );
				byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt( tempBytes, true );
				// Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
				// If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
				// Comment out the next line and the corresponding one in the DecryptString function.
				Array.Reverse( encryptedBytes );
				// Why convert to base 64?
				// Because it is the largest power-of-two base printable using only ASCII characters
				stringBuilder.Append( Convert.ToBase64String( encryptedBytes ) );				
			}			
			return stringBuilder.ToString();
		}
        /*
        public string EncryptString1(string inputString, string xmlString)
        {
            xmlString = "<RSAKeyValue><Modulus>sTVCkKrgJpVOZbW3iQUy1Y6AnUce40mzPqkXwYpZB0dT2I/BQoYEaZOmXV6Qs41k7pwtAPiMb7za65Bsn9ervsiyXF1fTHKN0t5ggdXcoMHmOK8dx2hesXhByH3BYEIrNPL8KGTTmZORm1CUm6b1SsYAuwKAAMMULPCglcT+59s=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            // TODO: Add Proper Exception Handlers
            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(1024);
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int keySize = 1024 / 8;
            byte[] bytes = Encoding.UTF32.GetBytes(inputString);
            // The hash function in use by the .NET RSACryptoServiceProvider here is SHA1
            // int maxLength = ( keySize ) - 2 - ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
            int maxLength = keySize - 42;
            int dataLength = bytes.Length;
            int iterations = dataLength / maxLength;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i <= iterations; i++)
            {
                byte[] tempBytes = new byte[(dataLength - maxLength * i > maxLength) ? maxLength : dataLength - maxLength * i];
                Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0, tempBytes.Length);
                byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt(tempBytes, true);
                // Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the DecryptString function.
                Array.Reverse(encryptedBytes);
                // Why convert to base 64?
                // Because it is the largest power-of-two base printable using only ASCII characters
                stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
            }
            return stringBuilder.ToString();
        }
        */
		public string DecryptString( string inputString, int dwKeySize, string xmlString )
		{
			// TODO: Add Proper Exception Handlers
            //xmlString = <RSAKeyValue><Modulus>sTVCkKrgJpVOZbW3iQUy1Y6AnUce40mzPqkXwYpZB0dT2I/BQoYEaZOmXV6Qs41k7pwtAPiMb7za65Bsn9ervsiyXF1fTHKN0t5ggdXcoMHmOK8dx2hesXhByH3BYEIrNPL8KGTTmZORm1CUm6b1SsYAuwKAAMMULPCglcT+59s=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
			RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider( dwKeySize );
			rsaCryptoServiceProvider.FromXmlString( xmlString );
			int base64BlockSize = ( ( dwKeySize / 8 ) % 3 != 0 ) ? ( ( ( dwKeySize / 8 ) / 3 ) * 4 ) + 4 : ( ( dwKeySize / 8 ) / 3 ) * 4;
			int iterations = inputString.Length / base64BlockSize;
			ArrayList arrayList = new ArrayList();
			for( int i = 0; i < iterations; i++ )
			{
				byte[] encryptedBytes = Convert.FromBase64String( inputString.Substring( base64BlockSize * i, base64BlockSize ) );
				// Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
				// If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
				// Comment out the next line and the corresponding one in the EncryptString function.
				Array.Reverse( encryptedBytes );
				arrayList.AddRange( rsaCryptoServiceProvider.Decrypt( encryptedBytes, true ) );				
			}			
			return Encoding.UTF32.GetString( arrayList.ToArray( Type.GetType( "System.Byte" ) ) as byte[] );
		}
        /*
        public string DecryptString1(string inputString, string xmlString)
        {
            // TODO: Add Proper Exception Handlers
            xmlString = "<RSAKeyValue><Modulus>sTVCkKrgJpVOZbW3iQUy1Y6AnUce40mzPqkXwYpZB0dT2I/BQoYEaZOmXV6Qs41k7pwtAPiMb7za65Bsn9ervsiyXF1fTHKN0t5ggdXcoMHmOK8dx2hesXhByH3BYEIrNPL8KGTTmZORm1CUm6b1SsYAuwKAAMMULPCglcT+59s=</Modulus><Exponent>AQAB</Exponent><P>3gk6S2U42rVm0j3j9TlCBZtBMUlwmR9zmjOXuKUU9Qvjh+vdXuRzXHjzAHzvAgYA9oFpetXfdyFQhBgpAhPy+Q==</P><Q>zFCZm3ugwBjbZ3FBSbn+HV7riaEaXmA3lV304O9HpLhrDhnE5KyO3/ZP27/vks5vZTC/Qz/H8fJFOg+zTB1Scw==</Q><DP>ON7AMaOBhnNEHMGBa8P4pxr2/brDvlSR9YMVb1PJJGKhKqU9FBsLeTn5c7yMM0Z6ZKkM7Utua0L7LnpJtqCJcQ==</DP><DQ>b9bG3qdjrQNuRkdmd5cKFMW6pNG8/2AnzOlrfB0+2FnisAtHDe2vH8VSnXWJDJFXxMpUR9mH91aoskmZ2dZLJQ==</DQ><InverseQ>AmzaBRoqY4v0A9V3YLnekX0xqpgNWuGROLey/xb0MCpliccbwR2hRvCTMRmU0JUUE1EdbgnaS6yKqg3NELM77Q==</InverseQ><D>aC4YqTZcOzKx+WfAtARjY1u4zz4dsaAFigQdHEJ6nqXXbEzvYG2rsGoGd4P97CFnQkR8zMJSxeowhibNRZektrVddTS8mEWwsJeyUe3C3HPx8AEsEg+FfotDWKKHuEvL2wiyQh7RqGmonnofjt9WgzHWaXVhVJBuInWgteBbenE=</D></RSAKeyValue>";
            RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(1024);
            //rsaCryptoServiceProvider=privKey;
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int base64BlockSize = ((1024 / 8) % 3 != 0) ? (((1024 / 8) / 3) * 4) + 4 : ((1024 / 8) / 3) * 4;
            int iterations = inputString.Length / base64BlockSize;
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < iterations; i++)
            {
                byte[] encryptedBytes = Convert.FromBase64String(inputString.Substring(base64BlockSize * i, base64BlockSize));
                // Be aware the RSACryptoServiceProvider reverses the order of encrypted bytes after encryption and before decryption.
                // If you do not require compatibility with Microsoft Cryptographic API (CAPI) and/or other vendors.
                // Comment out the next line and the corresponding one in the EncryptString function.
                Array.Reverse(encryptedBytes);
                arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(encryptedBytes, true));
            }
            return Encoding.UTF32.GetString(arrayList.ToArray(Type.GetType("System.Byte")) as byte[]);
        }*/
	}
}