export class StockData {
    constructor(public date: Date,
        public symbol: string,
        public open: number,
        public high: number,
        public low: number,
        public close: number,
        public adjustedClose: number,
        public volume: number,
        public previousClose: number) {

    }

    public get percentChangeFromPreviousClose(): number {
        if(this.previousClose == 0.0) {
            return 0.0;
        }
        return 100.0 * (this.close - this.previousClose) / this.previousClose;
    }
}
