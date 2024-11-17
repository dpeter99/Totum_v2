import {Services} from "@/App.tsx";
import { AuthContextProps } from "react-oidc-context";

export class UserService {
  private authService: AuthContextProps;

  constructor(opts: Services) {
    this.authService = opts.AuthProvider;
  }

  get userName() {
    return this.authService.user?.profile.name ?? '';
  }

  get profileImageUrl(): string {

    const profileImageUrl = this.authService.user?.profile.imageUrl as string | undefined;

    if (profileImageUrl) {
      return profileImageUrl;
    }

    return `https://i.pravatar.cc/250?u=${this.authService.user?.profile.name ?? 'asd'}`;
  }

  Logout() {
    this.authService.removeUser();
  }
}
