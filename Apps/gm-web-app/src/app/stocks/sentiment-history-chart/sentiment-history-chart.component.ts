import { Component, Input, OnInit } from '@angular/core';
import { StockRecommendationHistory } from '../stock-recommendation-history';

@Component({
  selector: 'gm-sentiment-history-chart',
  templateUrl: './sentiment-history-chart.component.html',
  styleUrls: ['./sentiment-history-chart.component.scss']
})
export class SentimentHistoryChartComponent implements OnInit {

  @Input() history: StockRecommendationHistory | undefined
  
  constructor() { }

  ngOnInit(): void {
  }

}
