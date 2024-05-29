using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckersGame
{
    public partial class Form1 : Form
    {
        const int mapSize = 8;  //переменная отчеющая за размер доски для шашек
        const int cellSize = 50; //переменная для определения размера клетки

        int currentPlayer;  //текущий игрок

        List<Button> simpleSteps = new List<Button>();

        int countEatSteps = 0;  //отвечает за подсчет количетства возможноых ходов
        Button prevButton; //записываю предыдущую кнопку
        Button pressedButton;
        bool isContinue = false;    //возможно ли после одного хода, продолжение?

        bool isMoving;

        int[,] map = new int[mapSize, mapSize];

        Button[,] buttons = new Button[mapSize, mapSize];

        Image whiteFigure;
        Image blackFigure;

        public Form1()
        {
            InitializeComponent();

            whiteFigure = new Bitmap(new Bitmap(@"C:\Users\qqvoder\Desktop\GAMEUP\CheckersGame-master\CheckersGame\Sprites\w.png"), new Size(cellSize - 10, cellSize - 10));
            blackFigure = new Bitmap(new Bitmap(@"C:\Users\qqvoder\Desktop\GAMEUP\CheckersGame-master\CheckersGame\Sprites\b.png"), new Size(cellSize - 10, cellSize - 10));

            this.Text = "Checkers"; //название формы

            Init();
        }

        public void Init() //Инициализация игры
        {
            currentPlayer = 1;      
            isMoving = false;           //находится ли шашка в процессе игры?
            prevButton = null;

            map = new int[mapSize,mapSize] {        
                { 0,1,0,1,0,1,0,1 },
                { 1,0,1,0,1,0,1,0 },
                { 0,1,0,1,0,1,0,1 },
                { 0,0,0,0,0,0,0,0 },
                { 0,0,0,0,0,0,0,0 },
                { 2,0,2,0,2,0,2,0 },
                { 0,2,0,2,0,2,0,2 },
                { 2,0,2,0,2,0,2,0 }
            };

            CreateMap(); 
        }

        public void ResetGame()
        {
            bool player1 = false;
            bool player2 = false;

            for(int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == 1)
                        player1 = true;
                    if (map[i, j] == 2)
                        player2 = true;
                }
            }
            if (!player1 || !player2)
            {
                this.Controls.Clear();
                Init();
            }
        }

        public void CreateMap()    //создание карты
        {
            this.Width = (mapSize + 1) * cellSize;     //Размеры    
            this.Height = (mapSize + 1) * cellSize;         //карты

            for(int i = 0; i < mapSize; i++)     //создам поле для игры в шашки
            {                                       //внутри этих циклов создам новую кнопку
                for (int j = 0; j < mapSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize); //изменяем позицию относительно индексов карты
                    button.Size = new Size(cellSize, cellSize); 
                    button.Click += new EventHandler(OnFigurePress); //обработчик
                    if (map[i, j] == 1)   //если текущий элемент карты равен 1, то в качестве картинки светлая фигура,
                                          //иначе темная
                        button.Image = whiteFigure;
                    else if (map[i, j] == 2) button.Image = blackFigure;

                    button.BackColor = GetPrevButtonColor(button); //перекидывам кнопки для окраса
                    button.ForeColor = Color.Red;

                    buttons[i, j] = button;

                    this.Controls.Add(button); //добавляем в форму
                }
            }
        }

        public void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;         //ф-ция, чтобы менять игроков
            ResetGame();
        }

        public Color GetPrevButtonColor(Button prevButton)     //ф-ция, чтобы менять цвет
        {
            if ((prevButton.Location.Y/cellSize % 2) != 0)
            {
                if ((prevButton.Location.X / cellSize % 2) == 0)  //вычисление индексов через позицию кнопки
                {
                    return Color.Gray;
                }
            }
            if ((prevButton.Location.Y / cellSize) % 2 == 0)
            {
                if ((prevButton.Location.X / cellSize) % 2 != 0)
                {
                    return Color.Gray;
                }
            }
            return Color.White;
        }

        public void OnFigurePress(object sender, EventArgs e)       //обработчик кнопок
        {
            if (prevButton != null)                   
                prevButton.BackColor = GetPrevButtonColor(prevButton);

            pressedButton = sender as Button; 

            if(map[pressedButton.Location.Y/cellSize,pressedButton.Location.X/cellSize] != 0 && map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize] == currentPlayer) //позиция карты в текующих индексах
            {                                                                                                                                  //индекс считывается через позицию кнопки                               
                CloseSteps();
                pressedButton.BackColor = Color.Red;
                DeactivateAllButtons();
                pressedButton.Enabled = true;
                countEatSteps = 0;
                if(pressedButton.Text == "D")
                ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize,false);
                else ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize);

                if (isMoving)
                {
                    CloseSteps();
                    pressedButton.BackColor = GetPrevButtonColor(pressedButton);
                    ShowPossibleSteps();
                    isMoving = false;
                }
                else
                    isMoving = true;
            }
            else
            {
                if (isMoving)   //этот элс выбирает куда ходить
                {
                    isContinue = false;
                      if (Math.Abs(pressedButton.Location.X / cellSize - prevButton.Location.X/cellSize) > 1)
                    {
                        isContinue = true;
                        DeleteEaten(pressedButton, prevButton);                        
                    }
                    int temp = map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize];
                    map[pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize] = map[prevButton.Location.Y / cellSize, prevButton.Location.X / cellSize];
                    map[prevButton.Location.Y / cellSize, prevButton.Location.X / cellSize] = temp;
                    pressedButton.Image = prevButton.Image;
                    prevButton.Image = null; //картинка у нажатой кнопки
                    pressedButton.Text = prevButton.Text;
                    prevButton.Text = "";
                    SwitchButtonToCheat(pressedButton);
                    countEatSteps = 0;
                    isMoving = false;                    
                    CloseSteps();
                    DeactivateAllButtons();
                    if (pressedButton.Text == "D")
                        ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize, false);
                    else ShowSteps(pressedButton.Location.Y / cellSize, pressedButton.Location.X / cellSize);
                    if (countEatSteps == 0 || !isContinue)
                    {
                        CloseSteps();
                        SwitchPlayer(); //смена игрока
                        ShowPossibleSteps();
                        isContinue = false;
                    }else if(isContinue)
                    {
                        pressedButton.BackColor = Color.Red;
                        pressedButton.Enabled = true;
                        isMoving = true;
                    }
                }
            }

            prevButton = pressedButton;
        }

        public void ShowPossibleSteps()     //показывает те шашки, у которых есть сьедобный ход
        {
            bool isOneStep = true;  //дамка или нет
            bool isEatStep = false;     //съедобный ход или нет
            DeactivateAllButtons();
            for(int i = 0; i < mapSize; i++)
            {               
                for (int j= 0; j < mapSize; j++)                        
                {
                    if (map[i, j] == currentPlayer)
                    {
                        if (buttons[i, j].Text == "D")
                            isOneStep = false;
                        else isOneStep = true;
                        if (IsButtonHasEatStep(i, j, isOneStep, new int[2] { 0, 0 }))
                        {
                            isEatStep = true;
                            buttons[i, j].Enabled = true;
                        }
                    }
                }
            }
            if (!isEatStep)
                ActivateAllButtons();
        }

        public void SwitchButtonToCheat(Button button)      //проверяю становится ли дамкой
        {
            if (map[button.Location.Y / cellSize, button.Location.X / cellSize] == 1 && button.Location.Y / cellSize == mapSize - 1) 
            {
                button.Text = "D";
                
            }
            if (map[button.Location.Y / cellSize, button.Location.X / cellSize] == 2 && button.Location.Y / cellSize == 0)
            {
                button.Text = "D";
            }
        }

        public void DeleteEaten(Button endButton, Button startButton)           //удаляю сьеденные шашки
        {
            int count = Math.Abs(endButton.Location.Y / cellSize - startButton.Location.Y / cellSize);
            int startIndexX = endButton.Location.Y / cellSize - startButton.Location.Y / cellSize;
            int startIndexY = endButton.Location.X / cellSize - startButton.Location.X / cellSize;
            startIndexX = startIndexX < 0 ? -1 : 1;
            startIndexY = startIndexY < 0 ? -1 : 1;
            int currCount = 0;
            int i = startButton.Location.Y / cellSize + startIndexX;
            int j = startButton.Location.X / cellSize + startIndexY;
            while (currCount < count-1)     //пробегую от 0 до coun - 1
            {
                map[i, j] = 0;
                buttons[i, j].Image = null;     //и удаляю все кнопки сьеденные в этом диапозоне
                buttons[i, j].Text = "";
                i += startIndexX;
                j += startIndexY;
                currCount++;
            }

        }

        public void ShowSteps(int iCurrFigure, int jCurrFigure,bool isOnestep = true)   //чистим пустые шаги , потом шоу диагнол и дубавляю в лист простые ходы, если кол-во шагов больше нуля, то оставля ток сьедобные ходы
        {
            simpleSteps.Clear();
            ShowDiagonal(iCurrFigure, jCurrFigure,isOnestep);
            if (countEatSteps > 0)
                CloseSimpleSteps(simpleSteps);
        }

        public void ShowDiagonal(int IcurrFigure, int JcurrFigure, bool isOneStep = false)      //высчитывает шаги на карте
        {
            int j = JcurrFigure + 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure + 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (IsInsideBorders(i, j))
                {
                    if (!DeterminePath(i, j))
                        break;
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
        }
        
        public bool DeterminePath(int ti,int tj)            
        {       //isconiune это показатель, есть ли серия сьедобных
            
            if (map[ti, tj] == 0 && !isContinue)            //если индексы нулевые и первый сьедобный ход, то красим кнопку как возможный шаг и добавлю в массив для обычных ходов
            {
                buttons[ti, tj].BackColor = Color.Yellow;
                buttons[ti, tj].Enabled = true;
                simpleSteps.Add(buttons[ti, tj]);
            }else
            {
                
                if (map[ti, tj] != currentPlayer)
                {
                    if (pressedButton.Text == "D")
                        ShowProceduralEat(ti, tj, false);
                    else ShowProceduralEat(ti, tj);
                }

                return false;
            }
            return true;
        }

        public void CloseSimpleSteps(List<Button> simpleSteps)      //передаем лист который надо закрыть
        {
            if (simpleSteps.Count > 0)
            {
                for (int i = 0; i < simpleSteps.Count; i++)
                {
                    simpleSteps[i].BackColor = GetPrevButtonColor(simpleSteps[i]);
                    simpleSteps[i].Enabled = false;
                }
            }
        }
        public void ShowProceduralEat(int i,int j,bool isOneStep = true) //строит следущий съедобный ход
        {
            int dirX = i - pressedButton.Location.Y / cellSize; //направления, относительно которых
            int dirY = j - pressedButton.Location.X / cellSize;                                 //сходили шашки
            dirX = dirX < 0 ? -1 : 1;       
            dirY = dirY < 0 ? -1 : 1;     //присваем к целым числам  
            int il = i;             
            int jl = j;
            bool isEmpty = true;            //есть возможность построить сьедобный ход?
            while (IsInsideBorders(il, jl)) //если карта в этих индексах не нулевая
            {
                if (map[il, jl] != 0 && map[il, jl] != currentPlayer) //и это не текущий игрок 
                { 
                    isEmpty = false;                                   //и это уже не пустая ячейка
                    break;
                }
                il += dirX;
                jl += dirY;

                if (isOneStep)
                    break;
            }
            if (isEmpty)
                return;
            List<Button> toClose = new List<Button>();
            bool closeSimple = false; //нужно ли закрывать несьедобные ходы
            int ik = il + dirX;         //увеличим на направление
            int jk = jl + dirY;
            while (IsInsideBorders(ik,jk))   //бегу по этому направлению и находится ли в границах
            {
                if (map[ik, jk] == 0 )          //если мужна сходить в эту ячейку
                {
                    if (IsButtonHasEatStep(ik, jk, isOneStep, new int[2] { dirX, dirY }))  
                    {
                        closeSimple = true;
                    }
                    else
                    {
                        toClose.Add(buttons[ik, jk]);
                    }
                    buttons[ik, jk].BackColor = Color.Yellow;
                    buttons[ik, jk].Enabled = true;
                    countEatSteps++;
                }
                else break;
                if (isOneStep) //если ток на одну клетку смотрим выходим из цикла
                    break;
                jk += dirY;
                ik += dirX;
            }
            if (closeSimple && toClose.Count > 0)   //если есть сьедобные, надо закрыть простые ходы
            {
                CloseSimpleSteps(toClose);
            }
            
        }
                                        
        public bool IsButtonHasEatStep(int IcurrFigure, int JcurrFigure, bool isOneStep, int[] dir)     //проверяю есть ли у текущей шашки вариант сделать ход
        {                           //                                       дамка или нет  направление относительно которого двигаться                          
            bool eatStep = false;
            int j = JcurrFigure + 1;
            //4 Цикла для 4 направлений
            for (int i = IcurrFigure - 1; i >= 0; i--)                  
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;  //отключение проверки в этом направлений
                                    //если первый сьедобный ход, тогда бреак, если нет, то смотрю во все стороны
                if (dir[0] == 1 && dir[1] == -1 && !isOneStep)break;     //шашки, которые не являются дамками, не могут возвращаться по доске
                if (IsInsideBorders(i, j))
                { //если не нулеаые элементы и не равен текущему игроку, возможно есть ход, поэтому в переменную тру
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j + 1)) //если ячейка уже вне границ, то фолс
                            eatStep = false;
                        else if (map[i - 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure - 1; i >= 0; i--)
            {
                if (currentPlayer == 1 && isOneStep && !isContinue) break;
                if (dir[0] == 1 && dir[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i - 1, j - 1))
                            eatStep = false;
                        else if (map[i - 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure - 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (dir[0] == -1 && dir[1] == 1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j - 1))
                            eatStep = false;
                        else if (map[i + 1, j - 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j > 0)
                    j--;
                else break;

                if (isOneStep)
                    break;
            }

            j = JcurrFigure + 1;
            for (int i = IcurrFigure + 1; i < 8; i++)
            {
                if (currentPlayer == 2 && isOneStep && !isContinue) break;
                if (dir[0] == -1 && dir[1] == -1 && !isOneStep) break;
                if (IsInsideBorders(i, j))
                {
                    if (map[i, j] != 0 && map[i, j] != currentPlayer)
                    {
                        eatStep = true;
                        if (!IsInsideBorders(i + 1, j + 1))
                            eatStep = false;
                        else if (map[i + 1, j + 1] != 0)
                            eatStep = false;
                        else return eatStep;
                    }
                }
                if (j < 7)
                    j++;
                else break;

                if (isOneStep)
                    break;
            }
            return eatStep;
        }

        public void CloseSteps()            //закрываю все  шаги которые были отрыты для текущей кнопки
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    buttons[i, j].BackColor = GetPrevButtonColor(buttons[i, j]);
                }
            }
        }

        public bool IsInsideBorders(int ti,int tj)      //находятся ли индексы в границах массива
        {
            if(ti>=mapSize || tj >= mapSize || ti<0 || tj < 0)
            {
                return false;
            }
            return true;
        }

        public void ActivateAllButtons()           //врубать кнопка
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    buttons[i, j].Enabled = true;
                }
            }
        }

        public void DeactivateAllButtons()          //выключаем все кнопки
        {
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    buttons[i, j].Enabled = false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
