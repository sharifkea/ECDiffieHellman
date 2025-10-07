# ECDiffieHellman

# C# Implementation Using ECDiffieHellmanCng
We'll use the ECDiffieHellmanCng class from System.Security.Cryptography (available in .NET Framework 4.6.2+ or .NET Core/5+). This class wraps Windows CNG (Cryptography API: Next Generation) for elliptic curve operations. By default, it uses the NIST P-256 curve.
Create a new C# Console App project in Visual Studio or via dotnet new console. Add this code to Program.cs. It simulates Alice and Bob generating keys, exchanging publics, and deriving the shared secret. For demo purposes, we'll print the public keys and the shared secret (as hex strings) and verify they match.

# Understanding Elliptic Curve Diffie-Hellman (ECDH) in C#
Elliptic Curve Diffie-Hellman (ECDH) is a key agreement protocol that allows two parties (e.g., Alice and Bob) to derive a shared secret key over an insecure channel without ever transmitting the secret itself. It's based on the mathematical difficulty of the elliptic curve discrete logarithm problem.
High-Level Steps in ECDH:
1.	Key Generation: Each party generates a private key (a random scalar) and computes a corresponding public key (a point on the elliptic curve: public = private * generator_point).
2.	Public Key Exchange: Alice sends her public key to Bob, and Bob sends his to Alice.
3.	Shared Secret Computation: 
o	Alice computes the shared secret as: shared = Bob's_public * Alice's_private.
o	Bob computes the shared secret as: shared = Alice's_public * Bob's_private.
o	Due to the properties of elliptic curves, these computations yield the same shared secret value.
4.	Key Derivation: The raw shared secret (an elliptic curve point) is typically hashed or processed into a usable symmetric key (e.g., for AES encryption).
This is different from "double ratchet" (which is part of the Signal Protocol and involves chaining keys for forward secrecy in messaging), but it's a foundational building block. We'll focus on a simple, symmetric demo here.

# Extending ECDH with Symmetric Encryption (AES-256-CBC)
Now, let's build on the ECDH key exchange to encrypt and send a message from Alice to Bob. We'll use the shared secret to derive a symmetric key (via SHA-256 hashing for simplicity— in production, use HKDF for better security). Then, Alice encrypts the plaintext "Hi, I am Alice." using AES-256-CBC mode, which requires a 32-byte key and a 16-byte initialization vector (IV). The encrypted message (IV + ciphertext) will be printed as hex for you to see.
Bob will then decrypt it using the same derived key and IV, recovering the original message.
Key Additions to the Code:
•	Key Derivation: SHA256.HashData(sharedSecret) to get a 32-byte AES key.
•	Encryption (Alice): Generate a random IV, encrypt the UTF-8 bytes of the message.
•	Transmission Simulation: Concatenate IV and ciphertext into a single byte array (as if sent over the wire).
•	Decryption (Bob): Extract IV and ciphertext, decrypt to get back the plaintext.
•	Output: Print the encrypted bytes (hex), and the decrypted message.

# Tree-based group Diffie-Hellman
The ECDiffieHellmanCng class does not provide built-in support for "Diffie-Hellman trees" (also known as tree-based group Diffie-Hellman or DH trees) or any form of group key agreement for multiple end users. It is designed exclusively as a pairwise key exchange primitive, enabling two parties to derive a shared secret over an insecure channel using elliptic curve cryptography (e.g., NIST P-256 by default). Key updates in multi-user scenarios, such as ratcheting for forward secrecy in groups or tree-structured resolutions for efficient key refreshes (as seen in protocols like MLS), would need to be implemented at a higher level by composing multiple pairwise ECDH operations.
For multi-user key management in .NET, we could extend our existing code by:
•	Performing pairwise ECDH for each pair in the group to derive per-pair secrets.
•	Using a library like OpenMLS (a Rust-based MLS implementation with .NET bindings via interop) or building a custom tree-based protocol on top of ECDiffieHellmanCng.
•	Handling key updates manually (e.g., via ephemeral key pairs for ratcheting).
