see [Generating a certificate](https://www.codeproject.com/Articles/1349071/Generating-a-certificate-using-a-Csharp-Bouncy-Cas)

see [examples](https://csharp.hotexamples.com/examples/Org.BouncyCastle.Asn1.X509/KeyUsage/-/php-keyusage-class-examples.html)

# Social Key Infrastructure

- self signed certificate is root certificate
- every person has a root certificate making every person a root CA (certificate authority)
- every person has one or more attestation certificates used to "vouch" for other people
- vouch - the act of attesting that a person is who they say they are.  Issues a certificate signed by an attestation certificate to the person being vouched for.

## Certificate Levels

1.  Root Certificate Authority (self-signed certificate)
2.  Root Certificate Authority + Intermediate Certificate Authority
      - Root Certificate Authority (self-signed certificate)
        - Intermediate Certificate Authority (certificate signed with Root Certificate Authority certificate)
3.  Root Certificate Authority + Intermediate Certificate Authority + Issuer Certificate Authority
      - Root Certificate Authority (self-signed certificate)
        - Intermediate Certificate Authority (certificate signed with Root Certificate Authority certificate)
          - Issuer Certificate Authority (certificate signed with Intermediate Certificate Authority certificate) 

## Data Model

