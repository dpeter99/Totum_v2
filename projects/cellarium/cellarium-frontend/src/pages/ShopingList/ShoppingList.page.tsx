import {useData} from "@/lib/app-host";
import {host, Services} from "@/App.tsx";
import {ShoppingListService} from "@/services/ShoppingListService.ts";
import {useMemo, useRef} from "react";
import {ShoppingListCreationDto} from "@/api";

class VM {
  service: ShoppingListService
  
  constructor(cradle: Services) {
    this.service = cradle.ShoppingListService;
  }
  
  async addShoppingList(newList: ShoppingListCreationDto) {
    await this.service.createShoppingList(newList);
  }
}


export const ShoppingListPage = () => {
  const vm = useMemo(()=>{
    return host.Container.build((cradle)=>new VM(cradle));
  }, [])
  
  const items = useData(vm.service.shoppingLists);
  
  const nameInput = useRef<HTMLInputElement>(null!);
  
  const addNewShoppingList = () => {
    vm.addShoppingList({name: nameInput.current.value});
  }
  
  return(
    <>
      <h1>Shopping List</h1>
      <div>
        <input name={'name'} ref={nameInput} />
        <button onClick={addNewShoppingList}>Add</button>
      </div>
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