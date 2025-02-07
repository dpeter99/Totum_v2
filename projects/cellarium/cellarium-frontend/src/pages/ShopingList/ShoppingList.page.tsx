import {useData} from "@/lib/app-host";
import {host, Services} from "@/App.tsx";
import {ShoppingListService} from "@/services/ShoppingListService.ts";
import { useMemo } from "react";

class VM {
  service: ShoppingListService
  
  constructor(cradle: Services) {
    this.service = cradle.ShoppingListService;
  }
}


export const ShoppingListPage = () => {
  const vm = useMemo(()=>{
    return host.Container.build((cradle)=>new VM(cradle));
  }, [])
  
  const items = useData(vm.service.shoppingLists);
  
  return(
    <>
      <h1>Shopping List</h1>
      {
        items.map((item) =>(
          <li key={item.id}>
            {item.name}
          </li>
        ))
      }
    </>
  );
  
};