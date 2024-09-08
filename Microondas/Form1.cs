using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Microondas
{

    public partial class Microondas : Form
    {

        bool executando = false;
        bool pausado = false;
        private CancellationTokenSource cancellationTokenSource;

        public Microondas()
        {
            InitializeComponent();
        }

        private void ValidadorTempo(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            int valorBotaoClicado = int.Parse(clickedButton.Tag.ToString());

            String novoTempoStr = ObterTempoAtualEmSegundos() + valorBotaoClicado.ToString();
            int novoTempo = int.Parse(novoTempoStr);

            if (novoTempo > 160 && novoTempo != 200)
            {
                MessageBox.Show("Tempo não deve ser maior que 2 minutos");
                return;
            }

            AtualizaTela(novoTempo);

        }

        private void AtualizaTela(int novoTempo)
        {
            if (novoTempo >= 100 && executando)
            {

                int minutos = novoTempo / 60;
                int segundos = novoTempo % 60;

                visorTempo.Text = $"{minutos:D1}:{segundos:D2}";
            }
            else if (novoTempo >= 100 && novoTempo < 160)
            {
                String temp = String.Concat(novoTempo);
                visorTempo.Text = temp[0] + ":" + temp[1] + temp[2];
            }
            else if (novoTempo == 60)
            {
                String temp = String.Concat(novoTempo);
                visorTempo.Text = 0 + ":" + temp[0] + temp[1];
            }
            else if (novoTempo == 160)
            {
                String temp = String.Concat(novoTempo);
                visorTempo.Text = temp[0] + ":" + temp[1] + temp[2];
            }
            else
            {
                novoTempo = Math.Min(novoTempo, 120);

                int minutos = novoTempo / 60;
                int segundos = novoTempo % 60;

                visorTempo.Text = $"{minutos:D1}:{segundos:D2}";
            }
        }

        private int ObterTempoAtualEmSegundos()
        {
            string[] tempoAtual = visorTempo.Text.Split(':');
            int minutos = int.Parse(tempoAtual[0]);
            int segundos = int.Parse(tempoAtual[1]);
            return (minutos * 60) + segundos;
        }

        private void Start(object sender, EventArgs e)
        {
            if (executando && !pausado)
            {
                int tempoAtual = ObterTempoAtualEmSegundos();
                if (tempoAtual + 30 <= 120)
                {
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource?.Dispose();
                    AtualizaTela(tempoAtual + 30);
                    Cronometro();
                } else
                {
                    MessageBox.Show("Tempo não deve ser maior que 2 minutos");
                    return;
                }
            }
            else
            {
                int tempoAtual = ObterTempoAtualEmSegundos();
                if (tempoAtual == 0)
                {
                    AtualizaTela(30);
                }
                executando = true;
                pausado = false;
                Cronometro();
            }

        }

        private void startPredefinido(object sender, EventArgs e)
        {
            reset();
            
            Button clickedButton = sender as Button;

            var serializer = new JavaScriptSerializer();
            AquecimentoParams parametros = serializer.Deserialize<AquecimentoParams>(clickedButton.Tag.ToString());

            potencia.Value = parametros.Potencia;
            executando = true;
            pausado = false;

            AtualizaTela(parametros.Tempo);
            instrucao.Text = parametros.Instrucao;
            Cronometro();
         }

        private async void Cronometro()
        {
            cancellationTokenSource = new CancellationTokenSource();

            int tempo = ObterTempoAtualEmSegundos();

            do
            {

                try
                {
                    await Task.Delay(1000, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                tempo--;
                AtualizaTela(tempo);
                AtualizaStringInformativa(tempo);
                if (tempo == 0)
                {
                    reset();
                    AtualizaStringInformativa(0);
                }
            }
            while (tempo > 0 && !pausado);
        }

        private void AtualizaStringInformativa(int tempoRestante)
        {
            int potenciaSelecionada = (int)potencia.Value;
            string mensagem;

            int quantidadePontos = tempoRestante * potenciaSelecionada;

            StringBuilder progresso = new StringBuilder();
            for (int i = 0; i < quantidadePontos; i++)
            {
                progresso.Append('.');
                if ((i + 1) % potenciaSelecionada == 0)
                {
                    progresso.Append(' ');
                }
            }

            if (tempoRestante > 0)
            {
                mensagem = progresso.ToString();
            }
            else if (tempoRestante == 0) {
                mensagem = "Aquecimento concluído";
            } else
            {
                mensagem = "";
            }

            labelTempo.Text = mensagem;
        }

        private void reset()
        {
            if (executando)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            potencia.Value = 10;
            instrucao.Text = "";
            executando = false;
            pausado = false;
            AtualizaTela(0);
        }

        private void CancelarOuPausar(object sender, EventArgs e)
        {
            if (executando && !pausado)
            {
                pausado = true;
            }
            else
            {
                reset();
                AtualizaStringInformativa(-1);
            }
        }

    }

    public class AquecimentoParams
    {
        public int Tempo { get; set; }
        public int Potencia { get; set; }
        public String Instrucao { get; set; }
        public AquecimentoParams(int tempo, int potencia, String instrucao)
        {
            Tempo = tempo;
            Potencia = potencia;
            Instrucao = instrucao;
        }

        public AquecimentoParams()
        {
        }
    }
}
