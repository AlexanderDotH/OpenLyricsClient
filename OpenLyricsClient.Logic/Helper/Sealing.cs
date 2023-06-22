using DevBase.Cryptography.BouncyCastle.AES;

namespace OpenLyricsClient.Logic.Helper;

public class Sealing
{
    private string _serverPublicKey;
    private string _simpleEncryptionKey;

    private DevBase.Cryptography.BouncyCastle.Sealing.Sealing _sealing;
    private AESBuilderEngine _aesBuilder;
    
    public Sealing()
    {
        this._serverPublicKey = "MIIBSzCCAQMGByqGSM49AgEwgfcCAQEwLAYHKoZIzj0BAQIhAP////8AAAABAAAAAAAAAAAAAAAA////////////////MFsEIP////8AAAABAAAAAAAAAAAAAAAA///////////////8BCBaxjXYqjqT57PrvVV2mIa8ZR0GsMxTsPY7zjw+J9JgSwMVAMSdNgiG5wSTamZ44ROdJreBn36QBEEEaxfR8uEsQkf4vOblY6RA8ncDfYEt6zOg9KE5RdiYwpZP40Li/hp/m47n60p8D54WK84zV2sxXs7LtkBoN79R9QIhAP////8AAAAA//////////+85vqtpxeehPO5ysL8YyVRAgEBA0IABBqSdbiWAMxcEig+rX1FlApI7pE/kPNUmejo5PXvElsf6pjHuDlBN4fYvpmaX6lncddAuNPnQmZ89Ogb95xwPnA=";
        this._sealing = new DevBase.Cryptography.BouncyCastle.Sealing.Sealing(this._serverPublicKey);

        this._simpleEncryptionKey = "g0o6Z9cInEYPJlLTNcm8iZND5AWeRfepa6xQhjpXd/k=";
        
        this._aesBuilder = new AESBuilderEngine()
            .SetKey(this._simpleEncryptionKey)
            .SetRandomSeed();

    }
    public string SimpleEncrypt(string data) => this._aesBuilder.EncryptString(data);

    public string SimpleDecrypt(string encryptedData) => this._aesBuilder.DecryptString(encryptedData);

    public string Seal(string data) => this._sealing.Seal(data);

    public string UnSeal(string sealedData) => this._sealing.UnSeal(sealedData);

    public string ServerPublicKey
    {
        get => _serverPublicKey;
    }
}