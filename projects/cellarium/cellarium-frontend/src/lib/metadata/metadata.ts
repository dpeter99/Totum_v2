
export type ObjectMeta<T> = {
    name: string,
    fields: FieldMeta<T, any>[], 
}

export type FieldType = 'string' | 'number' | 'boolean' | 'any';

export type FieldMetaBase<Obj, FieldT> = {
    name: string,
    type: FieldType,
    getter: (obj: Obj) => FieldT,
    setter?: (obj: Obj, value: FieldT) => void,
}

export type FieldMetaUi = {
    i18nKey: string,
    i18nKeySingular: string;
    i18nKeyPlural: string;
    icon: string | null;
    color: string | null;
}

export type FieldMetaValues<FieldT> = {
    potentialValues: FieldT[];
    sortingFunction?: (a: FieldT, b: FieldT) => number;
}

export type FieldMeta<Obj, FieldT> = FieldMetaBase<Obj, FieldT> & FieldMetaUi & FieldMetaValues<FieldT>;

export type FieldMetaCreation<Obj, FieldT> = Omit<FieldMetaBase<Obj, FieldT>,'type'> & Partial<FieldMetaUi> & Partial<FieldMetaValues<FieldT>>;

const makeField = <Obj>(data: FieldMetaCreation<Obj, any> & {type?: FieldType}): FieldMeta<Obj, any>=>{
    
    const defaultValues: FieldMeta<Obj, any> = {
        name: '',
        getter: () => null,
        type : 'any',

        i18nKey: '',
        i18nKeyPlural: '',
        i18nKeySingular: '',
        icon: null,
        color: null,

        potentialValues: [],
    }
    return Object.assign(data, defaultValues);
}

export const makeField_string =
    <Obj>(data: FieldMetaCreation<Obj, string>): FieldMeta<Obj, string> =>{
    return {
        ...makeField(data),
        type: 'string'
    }
}

export const makeField_number =
    <Obj>(data: FieldMetaCreation<Obj, number>): FieldMeta<Obj, number> =>{
        return {
            ...makeField(data),
            type: 'number'
        }
    }