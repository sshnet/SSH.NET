using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// old private key information/
    /// </summary>
    [TestClass]
    public class PrivateKeyFileTest : TestBase
    {
        [WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_EmptyFileName()
        {
            string fileName = string.Empty;
            var keyFile = new PrivateKeyFile(fileName);
        }

        [WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_PrivateKeyFile_StreamIsNull()
        {
            Stream stream = null;
            var keyFile = new PrivateKeyFile(stream);
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAoWv7yqk9zX9O5dG/+wOQbUnU8vlpwrfc/EFTKvOUY4GHOJ93
AodLRJshNxI0fQZnPWwSEdsFlB5y3NoJg13uGNWBikMlgj01Zzz6QTfnmpmvgWD2
30A5/METbBIIbXNk4Fx9jo9vMhpxn+yTOOq0grMicMfXaBL6xabUBogHVwpOmi50
cxYQuK3lXvYgYtctQOE+ZlZJY48HVUaSDGWLIRxIvJnP5r1fn+7fWlxt91mfq+qx
lE+HLOoaXMcoPAErZ5acGLiwKXl+iP9a6hi5LSQjxhXhQCUvjsvyNbCuyILyqkjE
opCkU+YSqAa2uBB0bKXuIIqkUBX+NNo36+BCqwIDAQABAoIBAECq8Pl0qbS8zs/V
ItMdz2tpC623a749SIOqa/YmFQl0J8bwY44Yw3edqkXUvkBkI1IBDJAorWh0dlGx
6+O76pn6VuYcFlfBy7YrQJqGGfvem+f5/4zavyBZ/TzrUIMAmqYjh41oOwTYgTKv
ZR+FL4G44BBMjyg7iklKl2ByeHZOoMGEpky8SBrt5RSOz+LrzXzEp9PmATrVTU38
Dpph8pk6JcrpS5qPrJyHYdLJ5KKnEe+++T/Kzc53dQSicMDcRlaIoYUSghANIOs4
3Z0kN3zkXuITYT2/fA7qn6EEQrHhIWW47gSf0ITv/DYvbqUVlmX/wPnScpuwpKhN
ucbwXHkCgYEA07Qw7/UODfO1zh5hsxWteejxJ9HQnImD5MbLwSllP9RYbHyKIe/M
+mFmrnU6bZ1O5pRYcqJu6keWxB9gmbKaOjlIS5Bz2tlm2cEujVm6zxBEB/VvxG2k
+2mpi1vaoQFnyXMB6Q7LUKmuR6qcSHadUABmBfnrWC1bwBzKpI8voo0CgYEAwzJ1
r02YQvl8RNJhY20+wfk1yfSEiHveJQzGVnlvxaXRr7gR4M4j4GaxyIYWZOTQcDpr
p0Iaq+E1T27ey+TSn/15wK0MLm/WkixQ/qXjTibweOOhbeMKVN0ou/n8FWu51B1i
ziU2FJnXhkk6wxSG32SnCdLU9cie1xcIhnSlSBcCgYEAhsi5Q3z0lsNsI6/KKheK
HAzHxL9bnt4agARlYzS0xr+uEFv2IgcnrN1oX3g5W+KEgl8+NLXgAf7UKUeE3DYO
5TTlJ17vtA0n40mQFuRjAEPt3FdR8nCQUpUCIby4ZDud1W8Ib1ZA8bkmQXCJEcWb
AH1Qd0uXn8s5GAX8qmqTPF0CgYAAiT7xgFEOvgitV7aUw1QMzFZte5JxnYV6rJJO
4n6AGHh+9w51g5ttnlqWpmehV0+LP13UU8Ym+rNeyHssLDC358ZR1SfXaM86D40Z
ZfM937WBX36uApWgrgrSmVkr5ePYxUvkLQ38+H8zCzhyGLhWHLyotj+HfUmSZanM
VL2veQKBgDpWFdCFgdE6ucaQuOSj4PbVCT2hTHuhuArP0NfSklcfeeGQedJfVupV
baH2S2V/3qOgcLO8pEZTSzEeX/QbcJzqjMz0yj0KQssGDxkbmlYltk2ZEM2DRM0B
Q4K/6SzJfIzSl/oYoB8xT0LY58qtBEurTZE81mmmiHV/gw6w+fQW
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)));
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_CBC()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: DES-CBC,BD35E157CDD07CAD

VU1aEcNJaFe1bhZ+sEzv70KQB94Pu2H+VoemLXtyIVzEQJV9+cymnRYjkgzykJ5d
s4J0vJMdiGtMH5lYRYEPQRNroOzJMwLngNyPiV8yLZARMR7tINdQyM7JBUFL4GfA
+0Jl/e7cZaCRJKAKQZu5L5DGuP6488tG7bKY9Rhzys21hPF+ck/8Gzfp1vC91AxF
zuy71e8ihEERF55DB5Ai1/lvEN15GUvkx14s0Oonfarxueu3dp/ch4P4PhzkY9Ci
7/ONnlpfrBR7bLajcXUq2XAO21ftDZejk5m6y0Z0bl0L+HpbJk2zFJGtwM+B+T3h
WkZqEK+RyTER67KUeecoKl297sh2YQtbxbdoTseUxktCb2/BSkpOHV5cgS64c3Ys
zGbk4dUmKv4HFs4VJ7HD3Ix6qAhMJyGQXOqze1c7ky9NoU2e05HdfgI/9We0EZQJ
PbequZZ4GOKnj4f70EbIEFyJldsJgijNCLKCafRYtygIs8zZkj/oE/S92iYZtesn
q2wxBAat3ZYn6tpkXx+4u9bSZ+U9OfYijxBv2x+BnAf0nQ1zNqmTOolEAIEMLS60
5WFWKVaOeqAP0Q185TEqpUFqcaTCycV2w1hN/XXuAiYqXaSYwg4ZtCmWmxbn20QH
SMvRuY6MZSNNncgoLbm+ySq1yP1Z46kRsU1ufAyN3Jue+6DVd8wFrEOXfLJnCbFk
Eykpd7vxzczHXWgrlakl2sPFy4ltcALc8ZhsmGO+goWNqwp4QPX4LMHeUsA9n86Q
ABNV+KkCyIzp6FsFVqeHCxD8EoE/MuiuMHy/n0oEGj7zp9Moq5DHrgQDmdKnhrAG
B2HqYlCH54LDKd3wlgT76/HJ8yviFZECY2q6Z1BnMzm2ikKLCyPBMKln2eaNUqss
YNt/16DUgxpThoXfS1T8zbYHGCO0niPGMy6LYWFu0XBVgKrFcl+D1mz9vGC2CBto
VA9YhIhjtlUWwmIaJAmlTXbHCXKWLjaK9/DbsnJOlUYG5XwTC+ntW24jZnDGkHzn
Vj87JDijGzziM6qte/JM4WrUKxd6Zrvl6AaTqGH3aPxZWdFsKETfJhfbBX3vjw5+
j6ltm4ZA+YXE2j8DLUQ8XMQlo66FUpiOj559aOXxfEb4HhJNrwo7VLsKuK56LdYb
keQmeQr+4HkDGjD0T8+0sAOx82B0TkgWYaCFU1wmzIsny+wiQmMoKJIR3T3Mzb07
9d5ncndS7DqcdAKMuG/F9w46QWW8G0veaJlV8ws7Ags1iBOxgTKrNsQNedB7B+zw
cTqikGnDgxVfiXOBRzHq7F3ZH9HcT4SSxuM6y2YN91C2DmPbtZAwlJX5nkORANy7
05kCAW/Md35jfVkZJsLsDDLNfRqukcWadkcKp3XvDB7/4WWFu8BrR9CHBY9j8hEt
FC4FTxZnnoDnnVg5sC8rYB6avD/MiomOUGOlHgM3MMk/Ta7fmioauCUBR5oXa/We
uhSoNyRY0/VZgE+fJ7P0Y5hzgnBDncVH5j57G0q4KTiTBDfuHBTLw+h5Htd5VBGS
5PwhKfrAvIwetRWMyRhfjixPDtcWZ2jx20fWCVxpPp+3MxBtuMgn7A==
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CBC()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: DES-EDE3-CBC,AF373EFF708479DF

iiuyZiL4qEI7nrJQL4gQZFyYYfji0GaNntX5rPrBlBLbwQySYq5d7ExLbv3tpB++
V1sQBHV9vcxNdq+uYqFofOuKHBXebIJLwluS+qVHC4sZoaYJxFIizl0Cf2RQzPPH
4XtAet8lrherbVm48YGAIcop9on87ILcOinNPf+0mT+wNLwFmymMkHmSByEMXU1m
7CF0GWY31rtFgfEZGw8KUjrolA0/JKUn7taLEHKHrmsXJKH2O/c7DvxMgL7ldBFk
mPVNQxsJfNGWsuU1/OzJjXWc5YYbziO2zKnJsCDpYOhATl92WABA7dPhI5jAJhWT
N1xo2JStdgy/wnyrye4WWzRZdPL47hO1bbQnpcG+VAHMsTSNngJuy1Y0pezsb7OW
2vmRM/I4QY2sPLhMOm9nbVfImJXIBukPbwpO5AtT3m+JwiyElnJcihDIaSkFTGOk
89i2tKPxWrl3obdo9CYy8ukrFMDHiOP4EGRAYFbxHLI20QDuyBEw5TR90ClCFE7P
ltM2NZRU/wCtF/4XEZ3P1KNVYu33d3hjr2J/fkba1xQ0xkCEfTXA8EgCA5ioQLie
WzdJndxuWMDZkHHeFbrLDmUBBtcv2ubC9eL6ULyeTk/v9F3gbnh46Oye3KUG64Ur
7H+NugCIDYyKdvcLl9O8JfspcNpg7ooLUliRMFCjIZI8G8V5fXqP1c4OTMaU0bGt
kBiVZeULxpKhPe8Inx4E33C1TAaHGwYezxzr9QP162ib9iWQ6hAWHizkzOYC8vRx
v0TKYR//1joVw7Wh27KOogoZEGy/gR3FZ5RxHai8inW22wlV9DobLJcib2dPkhMU
6ACAhGNwmjr1oHnCmINmKaTCxGsiqs7R13k+3qbTF5SfofOIdNzgP0rlo1X4ckuU
uA0jcNMk/R2qAn7xWDS1bdlBwj+85DlqWkKsrYuc9Jrll11HYCPoyIiY/kTahVLL
VJ+FDIjRaHEK0knjfVMtB+E3aZhaHpvLBRFNsU1POoLkqer7CyUAxD4UJSC87kIc
x8gX7kLR+ZTPuZ8Dx39jEpH+501bvoVxgDRstQkH0JTnOYJmLu/zobwAaX/+Xgdx
ALu2YJZJ8ikkFU7vbqUdzvCYKiM4srbMa+E/5wqIr6G4SSk8FupWIJWsAkczbEkH
PEfbVmWR0Lgh2i/RZU3kNqmUgV6DRfaML64z6dA13UIYIhVyg7ix6KYsJFJVKC5P
YY8No7y29htDbPqjWDZ1Y95/9foKAKhv+iNOTLfIa7H7iWYFTLYXMLt3Vsux6V9V
15+eUCe82yg7/MqcdL3IkX41AgQ6yijBWk761mbYJQ0TFNfPdtHxwKd9Rvj7nU+R
g0XhLT/b9gBNNvJeo02Hgs0axNo1WtMvd/HqYFpEx7SJf/ClniJd+kBBpn29zbLA
vaZAkxmIZc4rVeZEV8N14i9HfzmfjLM69wiMjfpO3H9nwzHHLhZCnzp483yTGKbc
EGa4dkBt3eYQDPkiK68vTt6fUfAWtiqhjmHCpOi+bZF/EfbTmz1zGIRNscFOl1Ln
bIk+F6YypdWnYjwQMr0e/RBZDVvsFH0XgHESq8hLEFXa6kWzQPIaVw==
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_128_CBC()
        {
            var key = @"
-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: AES-128-CBC,A8B993177AE83E5476797236484F7CB6

tgMoS/2KhL+p/KslJVfX1RYiJtu3MlHA9gLu4Cq+yG63Yo4eLJmxV11CxSFGhamx
mPhSh6KDxj5VV1BQm6jsY4sabZARZRuwwFnpGOHUsGCU7zfY3GTDhYfCHDhjXuhy
LBloxq+5DafcagqFG1AZyJEJV0ZxGb0jbxWU13frKIEr5WvUbNN/XamBo2UffmNu
Mumc/tl0sFih134PNAPvG53R99vuup5aAe7L72OmLEU3UqEH8Sm6i1Y0XKV/CB3I
BoO7lsu8bk+QWl0ZsPULXEejub+P12Q08ySM6wXHDsTex6D09vIYZsoaG0lzbglJ
LnMAXkVzvJOup5wYJgtpDJG1p1QtEvYjV94wyRoGWz9+7Nc9NlhgTeV02u6c2y0m
GZ/ok7Jkc1HGLi9N3MIdLQ56l25DChZv0VqS95tmoRrtAnxXpk5MVsSVdBH8OC7E
DyE++sdw6E5w3B596xJaJFnt3mz3+l5OdXd1/nASx3Oklq+nQ9GA2CKvOUYrQBqn
JO30lMTm77iFXXUTitXFN7Z5KLNoBV8exHKhWTEqKq+BjBTrUx9y/gDQ3Bz/T1hN
iuBtesIt5UdCpXKpG6klqfMZPDbRZCWCf6Kuby8+Lw5yCPgI6L5Wj2y69Cnbjva5
1vbmVQkXmjBUlM29huE9iI9Qsoc2Ws0TflXaHpO5ovN0mhuU9UpTXNwwIZbsGZBI
Fa8tToU8Z+xV/cktHJdwuFGWL9MwMvc6JdvRI/AZ9CVe/bahXlp/ETIX3xCs1dmm
MU+xh9t+fje8Ms7HNmHmbS4SQPnf0GA9BaQsA7HzblzE/xYh0nrLirXGVagUv7UJ
OPqBvxtrIbSbpgHjDkXcYlF1qdYBaTsh3CaDLcShBPz9GlC9XFh2S4imtJy+ESz9
wRtZMEZ4R9LchrK8ElEQbPvv2M9UsjLEi70idWKcHAuws05/IIQ6AgRnRoiuRb5Q
d71Hcbwt9X16PrfRSYRMNKoNmWQwTUvGFmIOc8HQYUPvHxdbTJYwha9rl+jecdd+
ExoUx0xhR+bXWoFPMqRzvGrFEOfjZ+JycSInhY/3cop7+m1xEk3nlc2ZH0Bm2E73
R1fkmJoj5/StLh2sNr+PCCVd2k70X/jfrgLvHqCtQPkdNlJnEV0tFchyaGealxCX
JtsAHIH4YgE5Ojnxb3aFJAsELKzTHYalR2pz1rwR1LWeT2bBpGq7R7Mh2tUg7BKc
1I9JfgzGi8X2+o+UHnzXPoVEEY1Snxkbni2jmQ06uoLaTow3qwUPExGE2J7CMOxZ
KYUy2T1aGGnIIzhYPslPU5BPEPmTDukk76B5Rvam0fvXwE4mv4YR0kb/XHYN7cEl
InDGFKnTcgLJfU/B+IxVoG9ZYIndlCgia1YMeXR+mqX0EdzCYmFxMNqW6QD5uzoC
LTNOQtqwiDAvWbLZcqkzT6YnGjN6TBJEnVY4nvZRpyHxFN5qcQnjfMNqc/V47H0u
GHy3G9CPTezdd7CGtb2sfDrls446rS8R9Zm15/2Y8sxysYXGZC/yv8a4QTnJn4lX
omJXeM2qa9U4lK44MhtHj+eLveKQokkJEHN1yeRUMWcbb4r2QSzbpwjjfna0QKBP
-----END RSA PRIVATE KEY-----
";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "tester");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_192_CBC()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: AES-192-CBC,0E34605476FC4C57886CE6350CD6F61E

yOzUZ81fSG2jBfhe7h/f2Uy7K86+VZFXVG3Y3qdQe6HD6kvQgQJLN+g8OVHan7vA
CYnKImqS5fiTIwvsMxzVWtCeY2kJ9BMNeNIoLXebD5RV93lKPBgl0IldrkfJrg0w
VNPkCHLFHWIOUA/bxYiXEeLRHuvndSkYH/JCzESiquf51chU4CIEmrbuvtvLXKta
YPO9eCw0PP1QKO/fJVrUHfp6Bsvai/J0+PG19XzkKCMsewG0MkAqpkzQrAjaCJyj
IwiqxxV72FIQDITgIQf5sY2yPzya/TjnBpu4m9D7TzQhCw9J2EDx61qvvKt3BDyq
DahaY6/T/wgcYvdoJ3RzOiWmSLvXNc1PNVY9OfQXG2EtJa/xi2BHyeHgo/Kib7Wr
nfaKgM7V998bLjiUnLK9CoD2WG3oSIrkAieNYbaD+RvU/mR/TNp5xMLMVJYduZo+
EeyooM1a29WjOU8LPZ/AwNS4/DHnzEG1UBsUbKtyzQHiKU3JoW+5BkZ2tAs+VEWo
b5Jx9XCKfL9JepP0Ti2adAlPYU9jH6YzTpZZfu+sd2+X/N8obYssUP6bFSB0KWL0
s/nx7DKzbXo4P+Lm04FpmeqmP3h4lhsInoozr0tP3JJKTb99kGV480eVdqDuV+Af
WJ4HLKlWxu8aHNnYb8/ATWEvh0Li0Qmx/ok8Ixa4XmHv2W0hjk7yhNewLpIwQ35X
qJgQph96gGzMGZQ3r/TjNd7YiAfPishOC/TnitIZzC/Es+zZ7QsGgBP4j1hPLAjs
OTrOvwkLuGUOoI9jJwm6343ZeGBFuWXJu0CSk4OF1EUMNziMUz9Rfw6xHSAnzff/
YuQwDx+EtZLyyxFsfdhLjwcR38R6WoFqeDPpb3i6B5dYhHG6w60Yq5X1J4EnJgLL
4zYqVLoC6rvUuslFX2EFGrTTZP/7qRi5H3ZhNzMf/KDVbTyhzzHx27LR/8vbScA0
IFp5NhmbmBbH7PmEF3UoVwSn/u+5iF8dmuhuOqNidImAULLCf5qCSbJNvagk9sSf
c6SdbX+EOkF6nTZYfSA0lT2u7rSfFw7khn6f+/ySRlQra7v3MBeeIuqrHEamlc73
4hPKnl8eGcT3XvrpMnuiYADjJ4qOYzwx3YiKGqvdGzqd+ML6f2Tk31N7E53UDvSR
96gYz7IiBEkbaQSV1iZ95iiv/0m2J8B7VR5FfwvLltrmD10Alq97Gpj7HUH/Yy+8
Wu4ADp6wdSWXMki2+QUxucIqWJ4nViE6K4FJy3SbPwtJWxiLkLQibGxdZl0jHDOl
F/FuWxjizKtOwn9rzQ+viVvYaQW1QTv5kS0d0L7FV2J8lQbsr8T86jUAJRASt5Cf
/+Z628xFSwDZjITOjhHT2GJqho/eW4h/naWgMRoThFI2l+o8Ko+kBFZ+fvmnY+MU
VcueeowDqsPawOj2YaaifvOzzsoP4C6Uu7K8UwAXE7gnKRWjIB4EJZLEaTXBpIUM
BcBcYCqd1X+JFQxp7fID+EGxlMfTjdZM4c51y67EHzMquZSiLEGBQgE8KiJclsIN
/PjY3hlwcZpyTEnqTYLnhL5SG/1fskUWPLaJ66u+aGoo9bcfOq9AE0yzPsl6MCes
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "12345");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_256_CBC()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: AES-256-CBC,063DE67AE11456C89BCE9D4A21BE3DFB

6mS1GhCjAg5mEwMFcKRJwg1uxCeY3ekJNCQewIN9NSI5A8prBOQ+JSyWAsn6c3Gw
OeRyur+5dxMFdt5Hz1CBi9EePvhVyMry7U5U86BWB0HgtDAD02b324sfc6Wk+kj5
PZvuKyXDiqdwy0rsbBUT+bLtXjCI4Ws1k/KbbF0OqGhFJJvErNU5x8zMD9mqp92R
D8ZZ/F8Sks3V/JeUisAF86sgMfVCELJobn5Zq/IaUyzQwC6IEL+Sy5fSBB5NHiex
NDIJg2RW79uLbufCpuoMPS/GKydf4dq0L5MwvKeqtUgf9Wddc+ZAE4+q1Xz/T8iN
3IMqsQfVbYjVK7uTaVGKH+Ew77Qryj01Vg+zyzdf4UwOV3XXQKLVCjNxpMCVtoq7
S45M3Ad7598vb7ooa/BFCIcEM8TkuzPnuttLqjzXEzUcA5kqm3kV14IKtlexBfNT
tarbidlZcOinvJaoIT3baP4rVnEWDKcxpc+UzNU5RRty6l0zpRmw/9RQ5+FKreh8
eXDHD8TT8ArdaREFM8J2OGpkmIK5sLhhYi9gnTopmKIHn8OAXusmQosEOzS6kGxk
aFtZezXSCBGgXp5RsrBGGx3oXWHGuWbEFXAq+M7PKXMQe5rLRv6sQdfTFSB5hgNK
82P8UzV1wWtAX4JYAhRh2zA8agY2arbNvbjRyjSbp9HNVBgSbVQ60JInesOqLxEg
XURuCYp4F8AeHzyO805MTNpcX7PZT2kOxp9sKKABJ9BJ0RoSWa0LJqXzGCHvrExE
g7XY/ZfDFZlPLbQnrOgVlYh7pzyfyKB74/oXHkonAisRfsgnQ87yT2DmcHNP6Cek
eae2nrpx2yn9Bf8rYdpmJgNxduO8IZvpn84xEyPqK+FbQsdOefBvsg5TgfzETkh/
SJjzbqCTDa3XHEUCInixo/wT7FxT8KR9vk43FGPNVRUvPB2GNxe9ZwLYIir64hcQ
CpdA3ipVx4/jVzWQH8KXG9UP9TDAKXEvbndLnr2taPnUdAnznwHN2EkfzS/PrFG4
/j3l1+VY2AyRybbCTI2iuwJPnKdxOR5oWW6I2Ksfq93Oy+NQz/zasjyNpCZBZWds
5gBmwiNk2Xzq7ikEVtVk3osOQRw/u9GbretfaT9jtClALL3DFbOzL4WxA+0NJqpd
NB2MohOJa1BJjdfh6x6EVhugH85Y9uYyz/MQj7piljAJY96190n3Q86b/7phfwuD
A/ixS42nqpyOPO+EjiWFerFVTJ3iBj7GXXOZGwCrZfpTbqE7OdTDnE3Vr4MO/Etq
kSDmJ/+4SFFh80YwYVERDNFdDxCYxx5AnxaBFwbqjzatTV/btgGVabIf6zm2L6aY
BJ5wnBZnRnsRaIMehDQmTjioMcyHBSMqId+LYQp+KFpBXqXQTEjJPnq+t4o2FF/N
8yoKR8BX6HXSO5qUndI8emwec1JeveiRai6SDnEz1EFfetYXImR290mlqt0aRjQk
t/HXRv+fmDQk5hJbCPICydcVSRyrbzxKkppVceEf9NwkBT1MBsOZIFJ3s3A9I72n
XPIab5czlgSLYA/U9nEg2XU21hKD2kRH1OF0WSlpNhN2SJFViVqlC3v36MgHoWNh
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "12345");
        }

        //[TestMethod]
        //[Owner("olegkap")]
        //[TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CFB()
        {
            var key = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: DES-EDE3-CFB,81C75CC63A21DFFB

7BCpj4mM2LTaWGP2f/IK8+Zd7XssLHtagETCURfg+x+IYhOOsW/qORNBeOL4lT8G
s8ymGJIMjNC0aGwJb214Kp19ajMlRN8IaHtw1QD3BYIxFSx35DSWd6WrECcdaJCm
FZ5y+rXf0NMUOUKg9xXF+Xnbucau3QN4NiLBB50oJyRIRco6Wy/9AB1yKrZsll4N
3+1XnnXZuanvIugi8TybUgzyrGE1dqwyGjHtN+bf8hWu8jrnx3AkjmzXJ+yiGbd4
w/JYfCzyVsEZuEzkn62johwNpwcuXFYEXxSSU444/TZf2BuuvvpkbCltkfvhOC3z
fp1DOtToaZadwHsH8laB+HPktisfetoPaQdqi/fGgqiERzDq9Xy7wY9JXdT65WeU
mh+USBy7mF6I57UgRM6AAZLvrJmG+hE8GYezThT9ZEnFyumrQgt8sTdWWFStYJcW
jlohuNO8c4IXwvXfVgafaIIAcFUcAKk/XgSLjMcn7YyBlaR6qIdwLLfRNEspv9mR
IF0M2ua4vZRLJfn+NOcs0n10v0jUFgMXoIqDr86OB3pW3ud/lET6bz6QYO3rNHW4
NtAmD2wwl66nuq2d9uLUSSkQj5spVDbFzfvnZCN3yl4hdyWlmzRJqybyr5xTIbT7
x5JF/eg3xq8weaZrFqq7r5uIhDYI7/sexxL9M/8nyV8COUYkDxxISbNpoDuCKbv8
fyIX92mGQtM8D7YftvCbEr8kw1fga9XhkDdOEuBzKZyIAD50xE39rFFMNNq8l8/Y
Gxo8zq0rW/IsrwvhWLLGtvmy68Be+WAi/mDHf6x4
-----END RSA PRIVATE KEY-----";
            new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(key)), "1234567890");
        }
    }
}