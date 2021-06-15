import { StockRecommendation } from "./stock-recommendation";

export class StockRecommendationHistory {
    constructor(public systemId: string,
        public symbol: string,
        public lastUpdated: Date,
        public lastRecommendation: StockRecommendation,
        public historicalData: Array<StockRecommendation>) {

        }
}
