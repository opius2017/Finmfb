// Biometric Authentication Service
export class BiometricService {
  async isAvailable(): Promise<boolean> {
    if (!window.PublicKeyCredential) {
      return false;
    }

    try {
      const available = await PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
      return available;
    } catch {
      return false;
    }
  }

  async register(username: string): Promise<boolean> {
    try {
      const challenge = await this.getChallenge();
      
      const credential = await navigator.credentials.create({
        publicKey: {
          challenge: this.base64ToBuffer(challenge),
          rp: {
            name: 'Soar-Fin+',
            id: window.location.hostname,
          },
          user: {
            id: this.stringToBuffer(username),
            name: username,
            displayName: username,
          },
          pubKeyCredParams: [
            { type: 'public-key', alg: -7 },  // ES256
            { type: 'public-key', alg: -257 }, // RS256
          ],
          authenticatorSelection: {
            authenticatorAttachment: 'platform',
            userVerification: 'required',
          },
          timeout: 60000,
          attestation: 'none',
        },
      }) as PublicKeyCredential;

      if (credential) {
        await this.saveCredential(username, credential);
        return true;
      }

      return false;
    } catch (error) {
      console.error('Biometric registration failed:', error);
      return false;
    }
  }

  async authenticate(username: string): Promise<boolean> {
    try {
      const challenge = await this.getChallenge();
      const credentialId = this.getStoredCredentialId(username);

      if (!credentialId) {
        return false;
      }

      const credential = await navigator.credentials.get({
        publicKey: {
          challenge: this.base64ToBuffer(challenge),
          allowCredentials: [{
            type: 'public-key',
            id: this.base64ToBuffer(credentialId),
          }],
          userVerification: 'required',
          timeout: 60000,
        },
      }) as PublicKeyCredential;

      if (credential) {
        return await this.verifyCredential(credential);
      }

      return false;
    } catch (error) {
      console.error('Biometric authentication failed:', error);
      return false;
    }
  }

  private async getChallenge(): Promise<string> {
    // In production, get from server
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return this.bufferToBase64(array);
  }

  private async saveCredential(username: string, credential: PublicKeyCredential): Promise<void> {
    const response = credential.response as AuthenticatorAttestationResponse;
    const credentialId = this.bufferToBase64(new Uint8Array(credential.rawId));
    
    localStorage.setItem(`biometric_${username}`, credentialId);
  }

  private getStoredCredentialId(username: string): string | null {
    return localStorage.getItem(`biometric_${username}`);
  }

  private async verifyCredential(credential: PublicKeyCredential): Promise<boolean> {
    // In production, verify with server
    return true;
  }

  private stringToBuffer(str: string): Uint8Array {
    return new TextEncoder().encode(str);
  }

  private base64ToBuffer(base64: string): Uint8Array {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return bytes;
  }

  private bufferToBase64(buffer: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < buffer.byteLength; i++) {
      binary += String.fromCharCode(buffer[i]);
    }
    return btoa(binary);
  }

  async removeBiometric(username: string): Promise<void> {
    localStorage.removeItem(`biometric_${username}`);
  }

  hasBiometric(username: string): boolean {
    return !!this.getStoredCredentialId(username);
  }
}

export const biometricService = new BiometricService();
