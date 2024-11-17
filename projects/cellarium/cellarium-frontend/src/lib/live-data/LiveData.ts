

type Observer<T> = (t:T, last: T | undefined) => void;

export class LiveData<T> {
    value: T;
    observers: Observer<T>[] = []
    
    constructor(initialValue: T,
                private onActive?: ()=>void,
                private onInactive?: ()=>void) {
        this.value = initialValue;
    }

    subscribe(observer: Observer<T>) {
        // Subscribed
        this.observers.push(observer);

        // Trigger onActive if first one subscribed
        this.onActive && this.observers.length === 1 && this.onActive();// eslint-disable-line no-unused-expressions

        // Immediatly notify of current value
        (this.value !== undefined) && observer(this.value, undefined)// eslint-disable-line no-unused-expressions

        // Unsubscribe
        return () => {
            this.observers = this.observers.filter(o => o !== observer)
            this.onInactive && this.observers.length === 0 && this.onInactive() // eslint-disable-line no-unused-expressions
        }
    }

    setValue(data: T) {
        return this.transition(()=>data);
    }

    transition(action: (prev: T)=>T) {
        const last = this.value
        // Transition to new value
        this.value = action(this.value)
        // Notify all observers
        console.log('value Changed: ', this.value, last);
        this.observers.forEach(observer => observer(this.value, last))
        return this.value
    }

}