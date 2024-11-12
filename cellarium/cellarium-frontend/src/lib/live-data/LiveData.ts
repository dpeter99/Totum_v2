

type Observer<T> = (t:T) => void;

export class LiveData<T> {
    value: T;
    observers: Observer<T>[] = []
    
    constructor(initialValue: T) {
        this.value = initialValue;
    }

    subscribe(observer: Observer<T>) {
        // Subscribed
        this.observers.push(observer);

        // Trigger onActive if first one subscribed
        //onActive && observers.length === 1 && onActive();// eslint-disable-line no-unused-expressions

        // Immediatly notify of current value
        (this.value !== undefined) && observer(this.value)// eslint-disable-line no-unused-expressions

        // Unsubscribe
        return () => {
            this.observers = this.observers.filter(o => o !== observer)
            // onInactive && observers.length === 0 && onInactive() // eslint-disable-line no-unused-expressions
        }
    }
}