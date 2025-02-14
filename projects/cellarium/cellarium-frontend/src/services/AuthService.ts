import {User as oidcUser, UserManager} from 'oidc-client-ts';
import {arachneConfig} from '@/config/auth';

export type User = oidcUser;

const userManager = new UserManager(arachneConfig.settings);


export async function getUser(): Promise<User | null> {
    return await userManager.getUser();
}


export async function isAuthenticated() {
    const token = await getAccessToken();


    return !!token;
}


export async function handleOAuthCallback(callbackUrl: string) {
    try {
        return await userManager.signinRedirectCallback(callbackUrl);
    } catch(e) {
        alert(e);
        console.log(`error while handling oauth callback: ${e}`);
    }
}


export async function sendOAuthRequest() {
    return await userManager.signinRedirect();
}


// renews token using refresh token
export async function renewToken() {
    return await userManager.signinSilent();
}


export async function getAccessToken() {
    const user = await getUser();
    return user?.access_token;
}


export async function logout() {
    await userManager.clearStaleState()
    await userManager.signoutRedirect();
}


// This function is used to access token claims
// `.profile` is available in Open Id Connect implementations
// in simple OAuth2 it is empty, because UserInfo endpoint does not exist
// export async function getRole() {
//     const user = await getUser();
//     return user?.profile?.role;
// }


// This function is used to change account similar way it is done in Google
// export async function selectOrganization() {
//     const args = {
//         prompt: "select_account"
//     }
//     await userManager.signinRedirect(args);
// }