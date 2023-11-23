
# Test Run
### Run Summary

<p>
<strong>Overall Result:</strong> ⚠️ Indeterminate <br />
<strong>Pass Rate:</strong> 98.17% <br />
<strong>Run Duration:</strong> 13m 30s  <br />
<strong>Date:</strong> 2023-11-22 09:35:17 - 2023-11-22 09:48:47 <br />
<strong>Framework:</strong> .NETCoreApp,Version=v7.0 <br />
<strong>Total Tests:</strong> 327 <br />
</p>

<table>
<thead>
<tr>
<th>✔️ Passed</th>
<th>❌ Failed</th>
<th>⚠️ Skipped</th>
</tr>
</thead>
<tbody>
<tr>
<td>321</td>
<td>0</td>
<td>6</td>
</tr>
<tr>
<td>98.17%</td>
<td>0%</td>
<td>1.83%</td>
</tr>
</tbody>
</table>

### Result Sets
#### Renci.SshNet.IntegrationTests.dll - 98.17%
<details>
<summary>Full Results</summary>
<table>
<thead>
<tr>
<th>Result</th>
<th>Test</th>
<th>Duration</th>
</tr>
</thead>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKey</td>
<td>2s 452ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKey_Connect_Then_Reconnect</td>
<td>2s 636ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKeyWithPassPhrase</td>
<td>1s 739ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKeyWithEmptyPassPhrase</td>
<td>963ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKey_MultiplePrivateKey</td>
<td>1s 714ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PublicKey_MultipleAuthenticationMethod</td>
<td>1s 725ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_KeyboardInteractiveAndPublicKey</td>
<td>1s 680ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_Password_ExceedsPartialSuccessLimit</td>
<td>1s 116ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_Password_MatchPartialSuccessLimit</td>
<td>1s 148ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_Password_Or_PublicKeyAndKeyboardInteractive</td>
<td>1s 709ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_Password_Or_PublicKeyAndPassword_BadPassword</td>
<td>4s 623ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PasswordAndPublicKey_Or_PasswordAndPassword</td>
<td>3s 546ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_PasswordAndPassword_Or_PublicKey</td>
<td>1s 277ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Multifactor_Password_Or_Password</td>
<td>1s 245ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>KeyboardInteractive_PasswordExpired</td>
<td>1s 308ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>KeyboardInteractiveConnectionInfo</td>
<td>1s 66ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>TripledesCbc</td>
<td>1s 474ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes128Cbc</td>
<td>1s 439ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes192Cbc</td>
<td>1s 469ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes256Cbc</td>
<td>1s 482ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes128Ctr</td>
<td>1s 478ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes192Ctr</td>
<td>1s 469ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Aes256Ctr</td>
<td>1s 449ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_CreateMoreChannelsThanMaxSessions</td>
<td>1s 149ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_DisposeAfterLossOfNetworkConnectivity</td>
<td>1s 478ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_DetectLossOfNetworkConnectivityThroughKeepAlive</td>
<td>1s 490ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_DetectConnectionResetThroughSftpInvocation</td>
<td>1s 389ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_LossOfNetworkConnectivityDisconnectAndConnect</td>
<td>2s 240ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_DetectLossOfNetworkConnectivityThroughSftpInvocation</td>
<td>1s 459ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_DetectSessionKilledOnServer</td>
<td>1s 340ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_HostKeyValidation_Failure</td>
<td>498ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_HostKeyValidation_Success</td>
<td>1s 129ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_HostKeyValidationSHA256_Success</td>
<td>1s 130ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_HostKeyValidationMD5_Success</td>
<td>1s 131ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Common_ServerRejectsConnection</td>
<td>811ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacMd5</td>
<td>1s 425ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacMd5_96</td>
<td>1s 450ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacSha1</td>
<td>1s 441ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacSha1_96</td>
<td>1s 458ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacSha2_256</td>
<td>1s 460ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>HmacSha2_512</td>
<td>1s 418ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshDss</td>
<td>2s 313ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsa</td>
<td>1s 446ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsaSha256</td>
<td>1s 411ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsaSha512</td>
<td>1s 439ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshEd25519</td>
<td>1s 398ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Curve25519Sha256</td>
<td>1s 469ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Curve25519Sha256Libssh</td>
<td>1s 470ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroup1Sha1</td>
<td>3s 121ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroup14Sha1</td>
<td>7s 649ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroup14Sha256</td>
<td>7s 587ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroup16Sha512</td>
<td>25s 529ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>DiffieHellmanGroup18Sha512</td>
<td></td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroupExchangeSha1</td>
<td>7s 598ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>DiffieHellmanGroupExchangeSha256</td>
<td>7s 635ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>EcdhSha2Nistp256</td>
<td>1s 804ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>EcdhSha2Nistp384</td>
<td>1s 983ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>EcdhSha2Nistp521</td>
<td>2s 265ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshDss</td>
<td>838ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsa</td>
<td>1s 460ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsaSha256</td>
<td>1s 461ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>SshRsaSha512</td>
<td>1s 428ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ecdsa256</td>
<td>849ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ecdsa384</td>
<td>3s 198ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ecdsa521</td>
<td>3s 150ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ed25519</td>
<td>3s 220ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Upload_And_Download_FileStream</td>
<td>162ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_DirectoryDoesNotExist</td>
<td>2s 245ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_DirectoryDoesNotExist</td>
<td>2s 239ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_FileDoesNotExist</td>
<td>2s 223ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_DirectoryInfo_DirectoryDoesNotExist</td>
<td>2s 229ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_DirectoryInfo_ExistingFile</td>
<td>3s 44ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_DirectoryInfo_ExistingDirectory</td>
<td>3s 130ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_DirectoryInfo_ExistingDirectory</td>
<td>3s 138ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_DirectoryDoesNotExist</td>
<td>2s 211ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_FileDoesNotExist</td>
<td>2s 252ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingDirectory</td>
<td>2s 270ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 31ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 39ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 19ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 58ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 47ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_FileInfo_ExistingFile</td>
<td>3s 70ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingDirectory</td>
<td>2s 261ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>2s 988ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>2s 990ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>3s 40ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>3s 38ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>3s 39ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Download_Stream_ExistingFile</td>
<td>2s 990ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_DirectoryDoesNotExist</td>
<td>2s 242ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_ExistingDirectory</td>
<td>3s 57ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_ExistingFile</td>
<td>3s 767ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_FileDoesNotExist</td>
<td>3s 856ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_FileDoesNotExist</td>
<td>3s 800ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_FileDoesNotExist</td>
<td>3s 800ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_FileDoesNotExist</td>
<td>3s 769ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileStream_FileDoesNotExist</td>
<td>3s 774ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_DirectoryDoesNotExist</td>
<td>2s 979ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_ExistingDirectory</td>
<td>3s 3ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_ExistingFile</td>
<td>3s 763ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>3s 800ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>4s 700ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>3s 806ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>3s 799ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>3s 860ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_FileInfo_FileDoesNotExist</td>
<td>3s 801ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_DirectoryInfo_DirectoryDoesNotExist</td>
<td>3s 15ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_DirectoryInfo_ExistingDirectory</td>
<td>3s 971ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_DirectoryInfo_ExistingDirectory</td>
<td>3s 951ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_DirectoryInfo_ExistingDirectory</td>
<td>3s 194ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Scp_Upload_DirectoryInfo_ExistingFile</td>
<td>2s 234ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Create_directory_with_contents_and_list_it</td>
<td>206ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Create_directory_with_contents_and_list_it_async</td>
<td>202ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Permission_Denied</td>
<td>163ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_UploadFile_FileStream (0)</td>
<td>782ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_UploadFile_FileStream (5242880)</td>
<td>1s 758ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ConnectDisconnect_Serial</td>
<td>1m 15s </td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ConnectDisconnect_Parallel</td>
<td>2m 28s </td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile</td>
<td>793ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Create_ExistingFile</td>
<td>796ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Create_DirectoryDoesNotExist</td>
<td>755ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Create_FileDoesNotExist</td>
<td>885ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_NoEncoding_ExistingFile</td>
<td>787ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_NoEncoding_DirectoryDoesNotExist</td>
<td>774ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_NoEncoding_FileDoesNotExist</td>
<td>789ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_NoEncoding_ExistingFile</td>
<td>796ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_NoEncoding_DirectoryDoesNotExist</td>
<td>784ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_NoEncoding_FileDoesNotExist</td>
<td>789ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_NoEncoding_ExistingFile</td>
<td>794ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_NoEncoding_DirectoryDoesNotExist</td>
<td>753ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_NoEncoding_FileDoesNotExist</td>
<td>783ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_Encoding_ExistingFile</td>
<td>795ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_Encoding_DirectoryDoesNotExist</td>
<td>760ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllLines_Encoding_FileDoesNotExist</td>
<td>782ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_Encoding_ExistingFile</td>
<td>787ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_Encoding_DirectoryDoesNotExist</td>
<td>769ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendAllText_Encoding_FileDoesNotExist</td>
<td>781ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_Encoding_ExistingFile</td>
<td>796ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_Encoding_DirectoryDoesNotExist</td>
<td>750ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_AppendText_Encoding_FileDoesNotExist</td>
<td>775ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_NoEncoding_ExistingFile</td>
<td>807ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_NoEncoding_DirectoryDoesNotExist</td>
<td>767ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_NoEncoding_FileDoesNotExist</td>
<td>784ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_Encoding_ExistingFile</td>
<td>793ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_Encoding_DirectoryDoesNotExist</td>
<td>763ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CreateText_Encoding_FileDoesNotExist</td>
<td>807ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_DownloadFile_FileDoesNotExist</td>
<td>765ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllBytes_ExistingFile</td>
<td>799ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllBytes_FileDoesNotExist</td>
<td>765ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllLines_NoEncoding_ExistingFile</td>
<td>794ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllLines_NoEncoding_FileDoesNotExist</td>
<td>767ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllLines_Encoding_ExistingFile</td>
<td>781ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllLines_Encoding_FileDoesNotExist</td>
<td>766ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllText_NoEncoding_ExistingFile</td>
<td>771ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllText_NoEncoding_FileDoesNotExist</td>
<td>779ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllText_Encoding_ExistingFile</td>
<td>789ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadAllText_Encoding_FileDoesNotExist</td>
<td>769ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadLines_NoEncoding_ExistingFile</td>
<td>784ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadLines_NoEncoding_FileDoesNotExist</td>
<td>765ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadLines_Encoding_ExistingFile</td>
<td>773ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ReadLines_Encoding_FileDoesNotExist</td>
<td>765ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllBytes_DirectoryDoesNotExist</td>
<td>770ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllBytes_ExistingFile</td>
<td>821ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllBytes_FileDoesNotExist</td>
<td>778ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_NoEncoding_DirectoryDoesNotExist</td>
<td>770ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_NoEncoding_ExistingFile</td>
<td>798ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_NoEncoding_FileDoesNotExist</td>
<td>790ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_Encoding_DirectoryDoesNotExist</td>
<td>807ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_Encoding_ExistingFile</td>
<td>807ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_IEnumerable_Encoding_FileDoesNotExist</td>
<td>787ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_NoEncoding_DirectoryDoesNotExist</td>
<td>758ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_NoEncoding_ExistingFile</td>
<td>795ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_NoEncoding_FileDoesNotExist</td>
<td>772ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_Encoding_DirectoryDoesNotExist</td>
<td>776ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_Encoding_ExistingFile</td>
<td>789ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllLines_Array_Encoding_FileDoesNotExist</td>
<td>772ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_NoEncoding_DirectoryDoesNotExist</td>
<td>769ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_NoEncoding_ExistingFile</td>
<td>818ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_NoEncoding_FileDoesNotExist</td>
<td>781ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_Encoding_DirectoryDoesNotExist</td>
<td>768ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_Encoding_ExistingFile</td>
<td>797ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_WriteAllText_Encoding_FileDoesNotExist</td>
<td>772ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginDownloadFile_FileDoesNotExist</td>
<td>759ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginListDirectory_DirectoryDoesNotExist</td>
<td>1s 502ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPath_DirectoryDoesNotExist</td>
<td>1s 578ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPath_FileDoesNotExist</td>
<td>790ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPath_ExistingFile</td>
<td>786ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_DirectoryDoesNotExist</td>
<td>1s 590ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_FileDoesNotExist</td>
<td>774ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsFalse_ExistingFile</td>
<td>770ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_DirectoryDoesNotExist</td>
<td>1s 602ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_FileDoesNotExist</td>
<td>824ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_BeginUploadFile_InputAndPathAndCanOverride_CanOverrideIsTrue_ExistingFile</td>
<td>792ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_UploadAndDownloadBigFile</td>
<td>9s 175ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_CurrentWorkingDirectory</td>
<td>821ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Exists</td>
<td>2s 399ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ListDirectory</td>
<td>2s 355ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ChangeDirectory_DirectoryDoesNotExist</td>
<td>1s 508ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_ChangeDirectory_DirectoryExists</td>
<td>2s 310ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_DownloadFile_MemoryStream</td>
<td>875ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SubsystemExecution_Failed</td>
<td>2s 272ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_ReadAndWrite</td>
<td>823ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_SetLength_ReduceLength</td>
<td>850ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_Seek_BeyondEndOfFile_SeekOriginBegin</td>
<td>931ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_Seek_BeyondEndOfFile_SeekOriginEnd</td>
<td>991ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_Seek_NegativeOffSet_SeekOriginEnd</td>
<td>891ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_Seek_Issue253</td>
<td>821ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_Seek_WithinReadBuffer</td>
<td>921ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SftpFileStream_SetLength_FileDoesNotExist</td>
<td>801ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Append_Write_ExistingFile</td>
<td>798ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Append_Write_FileDoesNotExist</td>
<td>790ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndMode_ModeIsCreate_FileDoesNotExist</td>
<td>806ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndMode_ModeIsCreate_ExistingFile</td>
<td>797ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsReadWrite_FileDoesNotExist</td>
<td>783ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsReadWrite_ExistingFile</td>
<td>788ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsWrite_ExistingFile</td>
<td>777ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_PathAndModeAndAccess_ModeIsCreate_AccessIsWrite_FileDoesNotExist</td>
<td>785ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_CreateNew_Write_ExistingFile</td>
<td>793ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_CreateNew_Write_FileDoesNotExist</td>
<td>791ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Open_Write_ExistingFile</td>
<td>808ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Open_Write_FileDoesNotExist</td>
<td>762ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_OpenOrCreate_Write_ExistingFile</td>
<td>789ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_OpenOrCreate_Write_FileDoesNotExist</td>
<td>797ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Truncate_Write_ExistingFile</td>
<td>790ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_Open_Truncate_Write_FileDoesNotExist</td>
<td>770ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_OpenRead</td>
<td>2s 54ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SetLastAccessTime</td>
<td>774ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SetLastAccessTimeUtc</td>
<td>782ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SetLastWriteTime</td>
<td>782ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Sftp_SetLastWriteTimeUtc</td>
<td>768ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Echo_Command_with_all_characters</td>
<td>157ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_ShellStream_Exit</td>
<td>2s 486ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>Ssh_ShellStream_IntermittendOutput</td>
<td></td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_CreateShell</td>
<td>1s 469ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_Command_IntermittendOutput_EndExecute</td>
<td>4s 772ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>Ssh_Command_IntermittendOutput_OutputStream</td>
<td></td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_DynamicPortForwarding_DisposeSshClientWithoutStoppingPort</td>
<td>1s 585ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_DynamicPortForwarding_DomainName</td>
<td>2s 21ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_DynamicPortForwarding_IPv4</td>
<td>1s 579ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_LocalPortForwardingCloseChannels</td>
<td>2s 49ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_LocalPortForwarding</td>
<td>1s 920ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_RemotePortForwarding</td>
<td>1s 577ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Ssh_ExecuteShellScript</td>
<td>2s 361ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_PortForwarding_Local_Stop_Hangs_On_Wait</td>
<td>4s 97ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_PortForwarding_Local_Without_Connecting</td>
<td>1ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_File_Upload_Download</td>
<td>570ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_Stream_Upload_Download</td>
<td>548ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_10MB_File_Upload_Download</td>
<td>2s 641ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_10MB_Stream_Upload_Download</td>
<td>2s 582ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_Directory_Upload_Download</td>
<td>4s 968ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_File_20_Parallel_Upload_Download</td>
<td>4s 778ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Scp_File_Upload_Download_Events</td>
<td>2s 411ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Root_Dont_Exists</td>
<td>144ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Root_With_Slash_Dont_Exists</td>
<td>161ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Subfolder_Dont_Exists</td>
<td>160ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Subfolder_With_Slash_Dont_Exists</td>
<td>150ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Which_Exists</td>
<td>158ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Which_Exists_With_Slash</td>
<td>160ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_CreateDirectory_In_Current_Location</td>
<td>146ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_CreateDirectory_In_Forbidden_Directory</td>
<td>149ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_CreateDirectory_Invalid_Path</td>
<td>161ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_CreateDirectory_Already_Exists</td>
<td>161ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_DeleteDirectory_Which_Doesnt_Exists</td>
<td>146ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_DeleteDirectory_Which_No_Permissions</td>
<td>168ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_DeleteDirectory</td>
<td>165ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_DeleteDirectory_Null</td>
<td>141ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Download_Forbidden</td>
<td>153ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Download_File_Not_Exists</td>
<td>161ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginDownloadFile_StreamIsNull</td>
<td>154ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginDownloadFile_FileNameIsWhiteSpace</td>
<td>148ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginDownloadFile_FileNameIsNull</td>
<td>148ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_EndDownloadFile_Invalid_Async_Handle</td>
<td>244ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Permission_Denied</td>
<td>149ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Not_Exists</td>
<td>149ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Current</td>
<td>167ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectoryAsync_Current</td>
<td>179ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Empty</td>
<td>151ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_Null</td>
<td>137ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ListDirectory_HugeDirectory</td>
<td>41s 311ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Change_Directory</td>
<td>411ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_ChangeDirectory_Null</td>
<td>144ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Call_EndListDirectory_Twice</td>
<td>152ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Rename_File</td>
<td>402ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_RenameFile_Null</td>
<td>154ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_RenameFileAsync</td>
<td>524ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_RenameFileAsync_Null</td>
<td>145ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_SynchronizeDirectories</td>
<td>308ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginSynchronizeDirectories</td>
<td>328ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Upload_And_Download_1MB_File</td>
<td>519ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Upload_Forbidden</td>
<td>168ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Multiple_Async_Upload_And_Download_10Files_5MB_Each</td>
<td>11s 531ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_Ensure_Async_Delegates_Called_For_BeginFileUpload_BeginFileDownload_BeginListDirectory</td>
<td>1s 802ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginUploadFile_StreamIsNull</td>
<td>159ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginUploadFile_FileNameIsWhiteSpace</td>
<td>140ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_BeginUploadFile_FileNameIsNull</td>
<td>148ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_EndUploadFile_Invalid_Async_Handle</td>
<td>338ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_Root_Directory</td>
<td>153ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_Invalid_Directory</td>
<td>150ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_File</td>
<td>154ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_File_Null</td>
<td>141ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_International_File</td>
<td>149ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Sftp_SftpFile_MoveTo</td>
<td>247ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Run_SingleCommand</td>
<td>145ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_SingleCommand</td>
<td>148ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>Test_Execute_OutputStream</td>
<td>163ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>Test_Execute_ExtendedOutputStream</td>
<td>136ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Timeout</td>
<td>5s 141ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Infinite_Timeout</td>
<td>10s 139ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_InvalidCommand</td>
<td>138ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_InvalidCommand_Then_Execute_ValidCommand</td>
<td>155ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_with_ExtendedOutput</td>
<td>143ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Reconnect_Execute_Command</td>
<td>259ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_ExitStatus</td>
<td>130ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Asynchronously</td>
<td>5s 187ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Asynchronously_With_Error</td>
<td>264ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Asynchronously_With_Callback</td>
<td>5s 187ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Asynchronously_With_Callback_On_Different_Thread</td>
<td>5s 182ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Execute_Command_Same_Object_Different_Commands</td>
<td>146ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_Result_Without_Execution</td>
<td>90ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_Get_Error_Without_Execution</td>
<td>111ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_EndExecute_Before_BeginExecute</td>
<td>83ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>BeginExecuteTest</td>
<td>16s 222ms</td>
</tr>
<tr>
<td> ⚠️ Skipped </td>
<td>Test_Execute_Invalid_Command</td>
<td>160ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_MultipleThread_Example_MultipleConnections</td>
<td>1s 686ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_MultipleThread_100_MultipleConnections</td>
<td>1s 761ms</td>
</tr>
<tr>
<td> ✔️ Passed </td>
<td>Test_MultipleThread_100_MultipleSessions</td>
<td>526ms</td>
</tr>
</tbody>
</table>
</details>

### Run Messages
<details>
<summary>Informational</summary>
<pre><code>
</code></pre>
</details>

<details>
<summary>Warning</summary>
<pre><code>
</code></pre>
</details>

<details>
<summary>Error</summary>
<pre><code>
</code></pre>
</details>



----

[Created using Liquid Test Reports](https://github.com/kurtmkurtm/LiquidTestReports)