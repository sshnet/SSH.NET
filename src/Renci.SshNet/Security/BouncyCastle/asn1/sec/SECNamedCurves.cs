using Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9;
using Renci.SshNet.Security.Org.BouncyCastle.Math;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Utilities.Encoders;

namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.Sec
{
    internal sealed class SecNamedCurves
    {
        /*
         * secp256r1
         */
        internal class Secp256r1Holder
            : X9ECParametersHolder
        {
            private Secp256r1Holder() {}

            internal static readonly X9ECParametersHolder Instance = new Secp256r1Holder();

            protected override X9ECParameters CreateParameters()
            {
                // p = 2^224 (2^32 - 1) + 2^192 + 2^96 - 1
                BigInteger p = FromHex("FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF");
                BigInteger a = FromHex("FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFC");
                BigInteger b = FromHex("5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B");
                byte[] S = Hex.Decode("C49D360886E704936A6678E1139D26B7819F7E90");
                BigInteger n = FromHex("FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551");
                BigInteger h = BigInteger.One;

                ECCurve curve = new FpCurve(p, a, b, n, h);
                X9ECPoint G = new X9ECPoint(curve, Hex.Decode("04"
                    + "6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296"
                    + "4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5"));

                return new X9ECParameters(curve, G, n, h, S);
            }
        }

        /*
         * secp384r1
         */
        internal class Secp384r1Holder
            : X9ECParametersHolder
        {
            private Secp384r1Holder() {}

            internal static readonly X9ECParametersHolder Instance = new Secp384r1Holder();

            protected override X9ECParameters CreateParameters()
            {
                // p = 2^384 - 2^128 - 2^96 + 2^32 - 1
                BigInteger p = FromHex("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFF");
                BigInteger a = FromHex("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFFFF0000000000000000FFFFFFFC");
                BigInteger b = FromHex("B3312FA7E23EE7E4988E056BE3F82D19181D9C6EFE8141120314088F5013875AC656398D8A2ED19D2A85C8EDD3EC2AEF");
                byte[] S = Hex.Decode("A335926AA319A27A1D00896A6773A4827ACDAC73");
                BigInteger n = FromHex("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC7634D81F4372DDF581A0DB248B0A77AECEC196ACCC52973");
                BigInteger h = BigInteger.One;

                ECCurve curve = new FpCurve(p, a, b, n, h);
                X9ECPoint G = new X9ECPoint(curve, Hex.Decode("04"
                    + "AA87CA22BE8B05378EB1C71EF320AD746E1D3B628BA79B9859F741E082542A385502F25DBF55296C3A545E3872760AB7"
                    + "3617DE4A96262C6F5D9E98BF9292DC29F8F41DBD289A147CE9DA3113B5F0B8C00A60B1CE1D7E819D7A431D7C90EA0E5F"));

                return new X9ECParameters(curve, G, n, h, S);
            }
        }

        /*
         * secp521r1
         */
        internal class Secp521r1Holder
            : X9ECParametersHolder
        {
            private Secp521r1Holder() {}

            internal static readonly X9ECParametersHolder Instance = new Secp521r1Holder();

            protected override X9ECParameters CreateParameters()
            {
                // p = 2^521 - 1
                BigInteger p = FromHex("01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                BigInteger a = FromHex("01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC");
                BigInteger b = FromHex("0051953EB9618E1C9A1F929A21A0B68540EEA2DA725B99B315F3B8B489918EF109E156193951EC7E937B1652C0BD3BB1BF073573DF883D2C34F1EF451FD46B503F00");
                byte[] S = Hex.Decode("D09E8800291CB85396CC6717393284AAA0DA64BA");
                BigInteger n = FromHex("01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFA51868783BF2F966B7FCC0148F709A5D03BB5C9B8899C47AEBB6FB71E91386409");
                BigInteger h = BigInteger.One;

                ECCurve curve = new FpCurve(p, a, b, n, h);
                X9ECPoint G = new X9ECPoint(curve, Hex.Decode("04"
                    + "00C6858E06B70404E9CD9E3ECB662395B4429C648139053FB521F828AF606B4D3DBAA14B5E77EFE75928FE1DC127A2FFA8DE3348B3C1856A429BF97E7E31C2E5BD66"
                    + "011839296A789A3BC0045C8A5FB42C7D1BD998F54449579B446817AFBD17273E662C97EE72995EF42640C550B9013FAD0761353C7086A272C24088BE94769FD16650"));

                return new X9ECParameters(curve, G, n, h, S);
            }
        }

        public static X9ECParameters GetByName(
            string name)
        {
            switch(name)
            {
                case "P-256":
                case "secp256r1":
                    return Secp256r1Holder.Instance.Parameters;
                case "P-384":
                case "secp384r1":
                    return Secp384r1Holder.Instance.Parameters;
                case "P-521":
                case "secp521r1":
                    return Secp521r1Holder.Instance.Parameters;
            }

            return null;
        }

        private static BigInteger FromHex(string hex)
        {
            return new BigInteger(1, Hex.Decode(hex));
        }
    }
}