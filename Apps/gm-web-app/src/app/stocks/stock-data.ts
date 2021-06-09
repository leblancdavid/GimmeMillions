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
}
