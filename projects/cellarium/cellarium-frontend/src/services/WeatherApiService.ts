import {LiveData} from "@/lib/live-data/LiveData.ts";
import {getResources} from "@/services/Api.ts";
import {Services} from "@/App.tsx";
import { AuthContextProps } from "react-oidc-context";

export type Weather = {
  temperatureC: number;
  summary: string;
}

export class WeatherApiService {
  private authService: AuthContextProps;

  constructor(opts: Services) {
    this.authService = opts.AuthProvider;
    this.getWeather();
  }

  public weather: LiveData<Weather[]> = new LiveData<Weather[]>([]);

  async getWeather() {
    if (!this.authService.isAuthenticated)
      return;

    const accessToken = this.authService.user!.access_token;
    const data = await getResources(accessToken);
    this.weather.setValue(data);
  }

}