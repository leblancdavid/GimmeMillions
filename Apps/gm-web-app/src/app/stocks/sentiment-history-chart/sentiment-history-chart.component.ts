import { Component, Input, OnInit } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { ChartDataSets, ChartOptions, ChartType } from 'chart.js';
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
      borderColor: '#1b603a',
      backgroundColor: 'rgba(196,210,83,0.25)',
    }
  ];
  lineChartLegend = true;
  lineChartPlugins = [];
  lineChartType: ChartType = 'line';
  selectedChart = 'sentiment';

  constructor() { 
    this.sentimentAxis = [];
    this.labelAxis = [];

  }

  ngOnInit(): void {
    this.setToSentimentChart();
  }

  updateChart(selection: MatSelectChange) {
    if(selection.value === 'confidence') {
      this.setToConfidenceChart();
    } else if(selection.value === 'price') {
      this.setToPriceChart();
    } else {
      this.setToSentimentChart();
    }
  }

  setToSentimentChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getDate() - n2.date.getDate());
      let data = [];
      this.lineChartColors = [
        {
          borderColor: '#1b603a',
          backgroundColor: 'rgba(196,210,83,0.25)',
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        data.push(sortedR[i].sentiment);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.sentimentAxis.push({
        data: data, label: 'Sentiment'
      });
    }
  }

  setToConfidenceChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getDate() - n2.date.getDate());
      let data = [];
      this.lineChartColors = [
        {
          borderColor: '#cb5115',
          backgroundColor: 'rgba(0,0,0,0)',
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        data.push(sortedR[i].confidence);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.sentimentAxis.push({
        data: data, label: 'Confidence'
      });
    }
  }

  setToPriceChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getDate() - n2.date.getDate());
      let s = [];
      this.lineChartColors = [
        {
          borderColor: 'black',
          backgroundColor: 'rgba(0,0,0,0)',
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        s.push(sortedR[i].lastData.close);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.sentimentAxis.push({
        data: s, label: 'Price'
      });
    }
  }
}
