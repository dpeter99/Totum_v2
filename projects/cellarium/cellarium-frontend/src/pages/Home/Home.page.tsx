import {useData, useService} from "@/lib/app-host";
import {host} from "../../App";

let i = 0;
export const Home = () => {
  const weatherAPI = useService(host, 'WeatherApiService');
  const weather = useData(weatherAPI.weather);

  console.log(`${i++}:${weatherAPI.weather.observers.length}`);

  return (
    <div>
      Home page
      {weather.map((w,i)=>(
        <div key={i}>
          {w.summary} : {w.temperatureC}
        </div>
      ))}
    </div>
  );
};