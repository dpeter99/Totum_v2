

type Observer<T> = (t:T, last: T | undefined) => void;

export class CancellationToken {
    isCancelled = false;
    cancel(){
        this.isCancelled = true;
    }
}

export class LiveData<T> {
    value: T;
    observers: Observer<T>[] = []
    
    cancelActivation = new CancellationToken();
    
    constructor(initialValue: T,
                private readonly onActive?: (cancellationToken: CancellationToken)=>void,
                private readonly onInactive?: ()=>void) {
        this.value = initialValue;
    }

    subscribe(observer: Observer<T>) {
        // Subscribed
        this.observers.push(observer);

        // Trigger onActive if first one subscribed
        this.observers.length === 1 && this.activate();// eslint-disable-line no-unused-expressions

        // Immediately notify of current value
        (this.value !== undefined) && observer(this.value, undefined)// eslint-disable-line no-unused-expressions

        // Unsubscribe
        return () => this.unsubscribe(observer)
    }
    
    private activate(){
        if(!this.onActive){
            return;
        }
        this.onActive(this.cancelActivation);
    }
    
    private unsubscribe(observer: Observer<T>) {
        this.observers = this.observers.filter(o => o !== observer)
        
        this.cancelActivation.cancel();
        this.onInactive && this.observers.length === 0 && this.onInactive() // eslint-disable-line no-unused-expressions
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