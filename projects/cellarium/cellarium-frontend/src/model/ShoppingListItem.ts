import {makeField_string, ObjectMeta} from "@/lib/metadata/metadata.ts";
import {Brand} from "@/utils";



export type ShoppingListItemID = Brand<string, 'ShoppingListItemID'>
export type ShoppingListItem = {
    id: ShoppingListItemID,
    title: string,
}

export const ShoppingListMeta: ObjectMeta<ShoppingListItem> = {
    name: 'ShoppingList',
    fields: [
        makeField_string({
            name: 'title',
            getter: obj => obj.title,
        }),
    ]
}