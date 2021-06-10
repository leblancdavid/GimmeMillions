import { Component, Input, OnInit } from '@angular/core';
import { ChartDataSets, ChartOptions } from 'chart.js';
import { Color, Label } from 'ng2-charts';
import { StockRecommendationHistory } from '../stock-recommendation-history';

@Component({
  selector: 'gm-sentiment-history-chart',
  templateUrl: './sentiment-history-chart.component.html',
  styleUrls: ['./sentiment-history-chart.component.scss']
})
export class SentimentHistoryChartComponent implements OnInit {

  @Input() history: StockRecommendationHistory | undefined
  @Input() maxLength: number = 30;

  sentimentAxis: ChartDataSets[];
  labelAxis: Label[];
  chartOptions = {
    responsive: true
  };

  lineChartColors: Color[] = [
    {
      borderColor: 'black',
      backgroundColor: 'rgba(255,255,0,0.28)',
    },
  ];
  lineChartLegend = true;
  lineChartPlugins = [];
  lineChartType = 'line';

  constructor() { 
    this.sentimentAxis = [];
    this.labelAxis = [];

  }

  ngOnInit(): void {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getDate() - n2.date.getDate());
      let s = [];
      
      debugger;

      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        s.push(sortedR[i].sentiment);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.sentimentAxis.push({
        data: s, label: 'Sentiment'
      });
    }
  }

}
