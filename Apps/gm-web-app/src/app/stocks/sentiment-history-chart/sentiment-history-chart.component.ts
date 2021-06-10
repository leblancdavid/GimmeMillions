import { Component, Input, OnInit } from '@angular/core';
import { ChartDataSets, ChartOptions } from 'chart.js';
import { Label } from 'ng2-charts';
import { StockRecommendationHistory } from '../stock-recommendation-history';

@Component({
  selector: 'gm-sentiment-history-chart',
  templateUrl: './sentiment-history-chart.component.html',
  styleUrls: ['./sentiment-history-chart.component.scss']
})
export class SentimentHistoryChartComponent implements OnInit {

  @Input() history: StockRecommendationHistory | undefined

  public sentimentAxis: ChartDataSets[];
  public labelAxis: Label[];
  public chartOptions: ChartOptions;
  constructor() { 
    this.sentimentAxis = [];
    this.labelAxis = [];
    this.chartOptions = {
      responsive: true
    };

  }

  ngOnInit(): void {
  }

}
