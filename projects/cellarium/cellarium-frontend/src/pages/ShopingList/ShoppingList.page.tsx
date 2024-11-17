import {useData} from "@/lib/app-host";
import {host, Services} from "@/App.tsx";
import {ShoppingListItem, ShoppingListItemService} from "@/services/ShoppingListItemService.ts";
import {LiveData} from "@/lib/live-data/LiveData.ts";
import { useMemo } from "react";

class VM {
  service: ShoppingListItemService
  
  public items = new LiveData<ShoppingListItem[]>([]);
  
  constructor(cradle: Services) {
    this.service = cradle.ShoppingListItemService;
    this.loadList();
  }
  
  async loadList(){
    const data = await this.service.getAllItems();
    this.items.setValue(data)
  }
  
  async addItem(){
    await this.service.addItem({
      title: "Add Item",
    })
    await this.loadList();
  }
}


export const ShoppingListPage = () => {
  const vm = useMemo(()=>{
    return host.Container.build((cradle)=>new VM(cradle));
  }, [])
  
  const items = useData(vm.items);
  
  return(
    <>
      <h1>Shopping List</h1>
      <button onClick={()=>vm.addItem()}>
        Add
      </button>
      {
        items.map((item) =>(
          <li key={item.id}>
            {item.title}
          </li>
        ))
      }
    </>
  );
  
};