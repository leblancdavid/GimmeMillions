export enum UserRole {
    SuperUser = 0,
    Administrator = 1,
    Default = 2
}

export class User {
    constructor(public id: number,
        public firstName: string,
        public lastName: string,
        public username: string,
        public password: string,
        public role: UserRole,
        public stocksWatchlistString: string) {
    }

    public authdata!: string;
    public get watchlist(): Array<string> {
        if(!this.stocksWatchlistString) {
            return new Array<string>();
        }
        return this.stocksWatchlistString.split(',');
    }

    public isEmpty() {
        return !this.isValid();
    }

    public isValid() {
        return this.username != '';
    }

    public addToWatchlist(symbol: string) {
        if(!this.watchlist.includes(symbol)) {
            if(this.watchlist.length == 0) {
                this.stocksWatchlistString = symbol;
            } else {
                this.stocksWatchlistString = this.stocksWatchlistString + ',' + symbol;
            }
        }
    }

    public removeFromWatchlist(symbol: string) {
        let wl = this.watchlist.filter(e => e !== symbol);
        this.stocksWatchlistString = wl.join(',');
    }
}
