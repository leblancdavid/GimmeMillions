export class StockRecommendation {
    constructor(public date: Date,
        public symbol: string,
        public systemId: string,
        public sentiment: number,
        public prediction: number,
        public lowPrediction: number,
        public previousClose: number,
        public predictedPriceTarget: number,
        public predictedLowTarget: number) {

    }

    public getRgb(intensity: number) {
        const range = 255 - intensity;
        const b = intensity;
        let g = 0;
        let r = 0;
        if(this.sentiment > 50.0) {
            r = intensity;
            g = ((this.sentiment - 50.0) / 50.0)*range + intensity; 
        } else {
            g = intensity;
            r = ((50.0 - this.sentiment) / 50.0) * range + intensity; 
        }
        return 'rgb(' + r + ',' + g + ',' + b + ')';
    }

    public getHsl(intensity: number) {
        let h = 105;
        let s = this.sentiment;
        let l = intensity;
        if(this.sentiment < 50.0) {
            h = 0;
            s = (50 - this.sentiment) * 2.0;
        } else {
            s = (this.sentiment - 50.0) * 2.0;
        }
        return 'hsl(' + h + ',' + s + '%,' + l + '%)';
    }
}
